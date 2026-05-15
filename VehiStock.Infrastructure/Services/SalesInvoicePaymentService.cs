using Microsoft.Extensions.Logging;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class SalesInvoicePaymentService : ISalesInvoicePaymentService
{
    private const decimal LoyaltyThresholdAmount = 5000m;
    private const decimal LoyaltyDiscountPercent = 10m;

    private readonly IPaymentServiceRepository _paymentServiceRepository;
    private readonly IKhaltiClient _khaltiClient;
    private readonly ILogger<SalesInvoicePaymentService> _logger;

    public SalesInvoicePaymentService(
        IPaymentServiceRepository paymentServiceRepository,
        IKhaltiClient khaltiClient,
        ILogger<SalesInvoicePaymentService> logger)
    {
        _paymentServiceRepository = paymentServiceRepository;
        _khaltiClient = khaltiClient;
        _logger = logger;
    }

    public async Task<InvoicePaymentInitiateResponse> InitiateAsync(
        string userId,
        int salesInvoiceId,
        InvoicePaymentInitiateRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _paymentServiceRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer profile was not found.");

        var invoice = await _paymentServiceRepository.GetSalesInvoiceForCustomerAsync(customer.CustomerId, salesInvoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Purchase invoice was not found.");

        if (invoice.PaymentStatus == PaymentStatus.Paid)
        {
            throw new InvalidOperationException("This invoice has already been fully paid.");
        }

        if (invoice.PaymentStatus == PaymentStatus.Cancelled)
        {
            throw new InvalidOperationException("This invoice has been cancelled.");
        }

        var amount = Math.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
        if (amount <= 0m)
        {
            throw new InvalidOperationException("Payment amount must be greater than zero.");
        }

        if (amount > invoice.BalanceDue)
        {
            throw new InvalidOperationException("Payment amount cannot exceed the balance due.");
        }

        var amountPaisa = (int)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        if (amountPaisa < 1000)
        {
            throw new InvalidOperationException("Khalti requires a minimum payment of NPR 10.");
        }

        var orderId = $"SLINV-{invoice.SalesInvoiceId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var orderName = $"Purchase {invoice.InvoiceNo} - {invoice.Vehicle.VehicleNumber}";

        var initiate = await _khaltiClient.InitiateAsync(new KhaltiInitiateInput
        {
            PurchaseOrderId = orderId,
            PurchaseOrderName = orderName,
            AmountPaisa = amountPaisa,
            CustomerName = customer.User?.FullName,
            CustomerEmail = customer.User?.Email,
            CustomerPhone = customer.User?.PhoneNumber
        }, cancellationToken);

        _logger.LogInformation(
            "Initiated Khalti payment for sales invoice {InvoiceId} amount={Amount} pidx={Pidx}",
            invoice.SalesInvoiceId, amount, initiate.Pidx);

        return new InvoicePaymentInitiateResponse
        {
            SalesInvoiceId = invoice.SalesInvoiceId,
            Pidx = initiate.Pidx,
            PaymentUrl = initiate.PaymentUrl,
            ExpiresAt = initiate.ExpiresAt,
            Amount = amount
        };
    }

    public async Task<SalesInvoiceLoyaltyResponse> SetLoyaltyAsync(
        string userId,
        int salesInvoiceId,
        SetSalesInvoiceLoyaltyRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _paymentServiceRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer profile was not found.");

        var invoice = await _paymentServiceRepository.GetSalesInvoiceForCustomerAsync(customer.CustomerId, salesInvoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Purchase invoice was not found.");

        if (invoice.PaymentStatus == PaymentStatus.Cancelled)
            throw new InvalidOperationException("Cancelled invoice cannot be updated.");

        if (invoice.AmountPaid > 0m || invoice.PaymentStatus is PaymentStatus.Partial or PaymentStatus.Paid)
            throw new InvalidOperationException("Loyalty can only be changed before any payment is made.");

        if (request.ApplyLoyalty && invoice.Subtotal <= LoyaltyThresholdAmount)
            throw new InvalidOperationException($"Loyalty discount applies only when purchase subtotal is above NPR {LoyaltyThresholdAmount:0}.");

        var currentInvoiceLevelDiscount = Math.Round(invoice.Subtotal * (invoice.DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var lineDiscountAmount = Math.Max(0m, Math.Round(invoice.DiscountAmount - currentInvoiceLevelDiscount, 2, MidpointRounding.AwayFromZero));

        var nextDiscountPercent = request.ApplyLoyalty ? LoyaltyDiscountPercent : 0m;
        var invoiceLevelDiscount = Math.Round(invoice.Subtotal * (nextDiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);

        var nextTotalAmount = Math.Round(invoice.Subtotal - (lineDiscountAmount + invoiceLevelDiscount) + invoice.TaxAmount, 2, MidpointRounding.AwayFromZero);
        if (nextTotalAmount < 0m)
            throw new InvalidOperationException("Loyalty update would result in a negative total amount.");

        invoice.DiscountPercent = nextDiscountPercent;
        invoice.DiscountAmount = Math.Round(lineDiscountAmount + invoiceLevelDiscount, 2, MidpointRounding.AwayFromZero);
        invoice.TotalAmount = nextTotalAmount;
        invoice.BalanceDue = Math.Round(invoice.TotalAmount - invoice.AmountPaid, 2, MidpointRounding.AwayFromZero);

        if (invoice.BalanceDue <= 0.005m)
            invoice.BalanceDue = 0m;

        invoice.PaymentStatus = ResolvePaymentStatus(invoice.TotalAmount, invoice.AmountPaid);

        await _paymentServiceRepository.SaveSalesInvoiceAsync(invoice, cancellationToken);

        return MapLoyaltyResponse(invoice);
    }

    private static PaymentStatus ResolvePaymentStatus(decimal totalAmount, decimal amountPaid)
    {
        if (totalAmount == 0m || amountPaid == totalAmount)
            return PaymentStatus.Paid;

        if (amountPaid == 0m)
            return PaymentStatus.Unpaid;

        return PaymentStatus.Partial;
    }

    private static SalesInvoiceLoyaltyResponse MapLoyaltyResponse(SalesInvoice invoice)
    {
        return new SalesInvoiceLoyaltyResponse
        {
            SalesInvoiceId = invoice.SalesInvoiceId,
            InvoiceNo = invoice.InvoiceNo,
            Subtotal = invoice.Subtotal,
            DiscountPercent = invoice.DiscountPercent,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            AmountPaid = invoice.AmountPaid,
            BalanceDue = invoice.BalanceDue,
            PaymentStatus = invoice.PaymentStatus.ToString()
        };
    }
}
