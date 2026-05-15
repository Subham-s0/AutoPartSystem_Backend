using Microsoft.Extensions.Logging;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class SalesInvoicePaymentService : ISalesInvoicePaymentService
{
    private readonly ICustomerPaymentRepository _customerPaymentRepository;
    private readonly IKhaltiClient _khaltiClient;
    private readonly ILogger<SalesInvoicePaymentService> _logger;

    public SalesInvoicePaymentService(
        ICustomerPaymentRepository customerPaymentRepository,
        IKhaltiClient khaltiClient,
        ILogger<SalesInvoicePaymentService> logger)
    {
        _customerPaymentRepository = customerPaymentRepository;
        _khaltiClient = khaltiClient;
        _logger = logger;
    }

    public async Task<InvoicePaymentInitiateResponse> InitiateAsync(
        string userId,
        int salesInvoiceId,
        InvoicePaymentInitiateRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerPaymentRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer profile was not found.");

        var invoice = await _customerPaymentRepository.GetSalesInvoiceForCustomerAsync(customer.CustomerId, salesInvoiceId, cancellationToken)
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
}
