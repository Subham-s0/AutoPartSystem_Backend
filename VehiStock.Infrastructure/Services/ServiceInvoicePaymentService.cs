using Microsoft.Extensions.Logging;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class ServiceInvoicePaymentService : IServiceInvoicePaymentService
{
    private readonly IPaymentServiceRepository _paymentServiceRepository;
    private readonly IKhaltiClient _khaltiClient;
    private readonly ILogger<ServiceInvoicePaymentService> _logger;

    public ServiceInvoicePaymentService(
        IPaymentServiceRepository paymentServiceRepository,
        IKhaltiClient khaltiClient,
        ILogger<ServiceInvoicePaymentService> logger)
    {
        _paymentServiceRepository = paymentServiceRepository;
        _khaltiClient = khaltiClient;
        _logger = logger;
    }

    public async Task<InvoicePaymentInitiateResponse> InitiateAsync(
        string userId,
        int serviceInvoiceId,
        InvoicePaymentInitiateRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _paymentServiceRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer profile was not found.");

        var invoice = await _paymentServiceRepository.GetServiceInvoiceForCustomerAsync(customer.CustomerId, serviceInvoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Service invoice was not found.");

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

        var orderId = $"SVCINV-{invoice.ServiceInvoiceId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var orderName = $"Service #{invoice.ServiceRecordId} - {invoice.Vehicle.VehicleNumber}";

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
            "Initiated Khalti payment for invoice {InvoiceId} service {ServiceId} amount={Amount} pidx={Pidx}",
            invoice.ServiceInvoiceId, invoice.ServiceRecordId, amount, initiate.Pidx);

        return new InvoicePaymentInitiateResponse
        {
            ServiceInvoiceId = invoice.ServiceInvoiceId,
            ServiceRecordId = invoice.ServiceRecordId,
            Pidx = initiate.Pidx,
            PaymentUrl = initiate.PaymentUrl,
            ExpiresAt = initiate.ExpiresAt,
            Amount = amount
        };
    }

    public async Task<InvoicePaymentVerifyResponse> VerifyAsync(
        string userId,
        InvoicePaymentVerifyRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Pidx))
        {
            throw new InvalidOperationException("pidx is required.");
        }

        var customer = await _paymentServiceRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer profile was not found.");

        var lookup = await _khaltiClient.LookupAsync(request.Pidx, cancellationToken);
        var khaltiStatus = lookup.Status.Trim();
        var mappedStatus = KhaltiPaymentStatusMapper.MapLookupStatusToPaymentStatusString(khaltiStatus);

        var orderKind = ParsePurchaseOrderKind(request.PurchaseOrderId);

        if (!KhaltiPaymentStatusMapper.ShouldApplyPayment(khaltiStatus))
        {
            return new InvoicePaymentVerifyResponse
            {
                ServiceInvoiceId = 0,
                SalesInvoiceId = orderKind == PurchaseOrderKind.Sales ? 0 : null,
                KhaltiStatus = khaltiStatus,
                MappedPaymentStatus = mappedStatus,
                Amount = lookup.TotalAmountPaisa / 100m,
                TransactionId = lookup.TransactionId,
                AlreadyProcessed = false,
                NewAmountPaid = 0m,
                NewBalanceDue = 0m,
                NewPaymentStatus = string.Empty
            };
        }

        if (string.IsNullOrWhiteSpace(lookup.TransactionId))
        {
            throw new InvalidOperationException("Khalti lookup did not return a transaction id for completed payment.");
        }

        var amountPaid = lookup.TotalAmountPaisa / 100m;

        if (await _paymentServiceRepository.PaymentExistsForKhaltiTransactionAsync(lookup.TransactionId, cancellationToken))
        {
            return new InvoicePaymentVerifyResponse
            {
                ServiceInvoiceId = 0,
                SalesInvoiceId = orderKind == PurchaseOrderKind.Sales ? 0 : null,
                KhaltiStatus = khaltiStatus,
                MappedPaymentStatus = mappedStatus,
                Amount = amountPaid,
                TransactionId = lookup.TransactionId,
                AlreadyProcessed = true,
                NewAmountPaid = 0m,
                NewBalanceDue = 0m,
                NewPaymentStatus = string.Empty
            };
        }

        var invoiceId = ParseInvoiceIdFromPurchaseOrderId(request.PurchaseOrderId)
            ?? throw new InvalidOperationException("Unable to determine the related invoice for this payment.");

        if (orderKind == PurchaseOrderKind.Sales)
        {
            return await ApplySalesInvoicePaymentAsync(
                customer.CustomerId,
                invoiceId,
                lookup,
                khaltiStatus,
                mappedStatus,
                amountPaid,
                cancellationToken);
        }

        return await ApplyServiceInvoicePaymentAsync(
            customer.CustomerId,
            invoiceId,
            lookup,
            khaltiStatus,
            mappedStatus,
            amountPaid,
            cancellationToken);
    }

    private async Task<InvoicePaymentVerifyResponse> ApplyServiceInvoicePaymentAsync(
        int customerId,
        int invoiceId,
        KhaltiLookupResult lookup,
        string khaltiStatus,
        string mappedStatus,
        decimal amountPaid,
        CancellationToken cancellationToken)
    {
        var invoice = await _paymentServiceRepository.GetServiceInvoiceForCustomerAsync(customerId, invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Service invoice was not found for this customer.");

        if (amountPaid <= 0m)
        {
            throw new InvalidOperationException("Khalti returned a non-positive payment amount.");
        }

        if (amountPaid - invoice.BalanceDue > 0.01m)
        {
            _logger.LogWarning(
                "Khalti payment {Pidx} amount {Amount} exceeds balance due {Balance} for service invoice {InvoiceId}. Capping to balance.",
                lookup.Pidx, amountPaid, invoice.BalanceDue, invoice.ServiceInvoiceId);
            amountPaid = invoice.BalanceDue;
        }

        invoice.AmountPaid = Math.Round(invoice.AmountPaid + amountPaid, 2, MidpointRounding.AwayFromZero);
        invoice.BalanceDue = Math.Round(invoice.TotalAmount - invoice.AmountPaid, 2, MidpointRounding.AwayFromZero);
        if (invoice.BalanceDue <= 0.005m)
        {
            invoice.BalanceDue = 0m;
        }

        invoice.PaymentStatus = KhaltiPaymentStatusMapper.ResolveInvoiceStatusAfterPayment(
            invoice.TotalAmount,
            invoice.AmountPaid);

        var payment = new Payment
        {
            ServiceInvoiceId = invoice.ServiceInvoiceId,
            CustomerId = customerId,
            PaymentType = PaymentType.Khalti,
            Amount = amountPaid,
            Notes = $"khalti_pidx:{lookup.Pidx};khalti_txn:{lookup.TransactionId}"
        };

        await _paymentServiceRepository.AddPaymentAndSaveAsync(payment, invoice, cancellationToken);

        _logger.LogInformation(
            "Recorded Khalti payment {Pidx} for service invoice {InvoiceId} amount {Amount}. Status={Status}.",
            lookup.Pidx, invoice.ServiceInvoiceId, amountPaid, invoice.PaymentStatus);

        return new InvoicePaymentVerifyResponse
        {
            ServiceInvoiceId = invoice.ServiceInvoiceId,
            KhaltiStatus = khaltiStatus,
            MappedPaymentStatus = invoice.PaymentStatus.ToString(),
            Amount = amountPaid,
            TransactionId = lookup.TransactionId,
            AlreadyProcessed = false,
            NewAmountPaid = invoice.AmountPaid,
            NewBalanceDue = invoice.BalanceDue,
            NewPaymentStatus = invoice.PaymentStatus.ToString()
        };
    }

    private async Task<InvoicePaymentVerifyResponse> ApplySalesInvoicePaymentAsync(
        int customerId,
        int invoiceId,
        KhaltiLookupResult lookup,
        string khaltiStatus,
        string mappedStatus,
        decimal amountPaid,
        CancellationToken cancellationToken)
    {
        var invoice = await _paymentServiceRepository.GetSalesInvoiceForCustomerAsync(customerId, invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Purchase invoice was not found for this customer.");

        if (amountPaid <= 0m)
        {
            throw new InvalidOperationException("Khalti returned a non-positive payment amount.");
        }

        if (amountPaid - invoice.BalanceDue > 0.01m)
        {
            _logger.LogWarning(
                "Khalti payment {Pidx} amount {Amount} exceeds balance due {Balance} for sales invoice {InvoiceId}. Capping to balance.",
                lookup.Pidx, amountPaid, invoice.BalanceDue, invoice.SalesInvoiceId);
            amountPaid = invoice.BalanceDue;
        }

        invoice.AmountPaid = Math.Round(invoice.AmountPaid + amountPaid, 2, MidpointRounding.AwayFromZero);
        invoice.BalanceDue = Math.Round(invoice.TotalAmount - invoice.AmountPaid, 2, MidpointRounding.AwayFromZero);
        if (invoice.BalanceDue <= 0.005m)
        {
            invoice.BalanceDue = 0m;
        }

        invoice.PaymentStatus = KhaltiPaymentStatusMapper.ResolveInvoiceStatusAfterPayment(
            invoice.TotalAmount,
            invoice.AmountPaid);

        var payment = new Payment
        {
            SalesInvoiceId = invoice.SalesInvoiceId,
            CustomerId = customerId,
            PaymentType = PaymentType.Khalti,
            Amount = amountPaid,
            Notes = $"khalti_pidx:{lookup.Pidx};khalti_txn:{lookup.TransactionId}"
        };

        await _paymentServiceRepository.AddSalesInvoicePaymentAndSaveAsync(payment, invoice, cancellationToken);

        _logger.LogInformation(
            "Recorded Khalti payment {Pidx} for sales invoice {InvoiceId} amount {Amount}. Status={Status}.",
            lookup.Pidx, invoice.SalesInvoiceId, amountPaid, invoice.PaymentStatus);

        return new InvoicePaymentVerifyResponse
        {
            SalesInvoiceId = invoice.SalesInvoiceId,
            KhaltiStatus = khaltiStatus,
            MappedPaymentStatus = invoice.PaymentStatus.ToString(),
            Amount = amountPaid,
            TransactionId = lookup.TransactionId,
            AlreadyProcessed = false,
            NewAmountPaid = invoice.AmountPaid,
            NewBalanceDue = invoice.BalanceDue,
            NewPaymentStatus = invoice.PaymentStatus.ToString()
        };
    }

    private enum PurchaseOrderKind
    {
        Unknown,
        Service,
        Sales
    }

    private static PurchaseOrderKind ParsePurchaseOrderKind(string purchaseOrderId)
    {
        if (string.IsNullOrWhiteSpace(purchaseOrderId))
        {
            return PurchaseOrderKind.Unknown;
        }

        var parts = purchaseOrderId.Split('-');
        if (parts.Length < 2)
        {
            return PurchaseOrderKind.Unknown;
        }

        if (string.Equals(parts[0], "SVCINV", StringComparison.OrdinalIgnoreCase))
        {
            return PurchaseOrderKind.Service;
        }

        if (string.Equals(parts[0], "SLINV", StringComparison.OrdinalIgnoreCase))
        {
            return PurchaseOrderKind.Sales;
        }

        return PurchaseOrderKind.Unknown;
    }

    private static int? ParseInvoiceIdFromPurchaseOrderId(string purchaseOrderId)
    {
        var kind = ParsePurchaseOrderKind(purchaseOrderId);
        if (kind is not (PurchaseOrderKind.Service or PurchaseOrderKind.Sales))
        {
            return null;
        }

        var parts = purchaseOrderId.Split('-');
        return int.TryParse(parts[1], out var invoiceId) ? invoiceId : null;
    }
}
