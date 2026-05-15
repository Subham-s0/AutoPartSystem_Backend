using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class CustomerPaymentService : ICustomerPaymentService
{
    private readonly ICustomerPaymentRepository _customerPaymentRepository;

    public CustomerPaymentService(ICustomerPaymentRepository customerPaymentRepository)
    {
        _customerPaymentRepository = customerPaymentRepository;
    }

    public async Task<PaginatedResponse<CustomerPaymentListResponse>> GetPaymentsPageAsync(
        string userId,
        CustomerPaymentQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerPaymentRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer profile was not found for this account.");

        NormalizeQuery(request);
        var payments = await _customerPaymentRepository.GetPaymentsPageAsync(customer.CustomerId, request, cancellationToken);

        return new PaginatedResponse<CustomerPaymentListResponse>
        {
            Items = payments.Items.Select(MapPayment).ToList(),
            PageNumber = payments.PageNumber,
            PageSize = payments.PageSize,
            TotalRecords = payments.TotalRecords,
            TotalPages = payments.TotalPages
        };
    }

    private static void NormalizeQuery(CustomerPaymentQueryRequest request)
    {
        request.PageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        request.PageSize = Math.Clamp(request.PageSize, 1, 50);
    }

    private static CustomerPaymentListResponse MapPayment(Payment payment)
    {
        if (payment.SalesInvoiceId.HasValue && payment.SalesInvoice is not null)
        {
            return new CustomerPaymentListResponse
            {
                PaymentId = payment.PaymentId,
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount,
                PaymentType = payment.PaymentType.ToString(),
                InvoiceKind = "Sales",
                SalesInvoiceId = payment.SalesInvoiceId,
                InvoiceReference = payment.SalesInvoice.InvoiceNo,
                VehicleNumber = payment.SalesInvoice.Vehicle.VehicleNumber,
                InvoicePaymentStatus = payment.SalesInvoice.PaymentStatus.ToString(),
                TransactionId = ExtractKhaltiTransactionId(payment.Notes)
            };
        }

        if (payment.ServiceInvoiceId.HasValue && payment.ServiceInvoice is not null)
        {
            return new CustomerPaymentListResponse
            {
                PaymentId = payment.PaymentId,
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount,
                PaymentType = payment.PaymentType.ToString(),
                InvoiceKind = "Service",
                ServiceInvoiceId = payment.ServiceInvoiceId,
                InvoiceReference = $"Service #{payment.ServiceInvoice.ServiceRecordId}",
                VehicleNumber = payment.ServiceInvoice.Vehicle.VehicleNumber,
                InvoicePaymentStatus = payment.ServiceInvoice.PaymentStatus.ToString(),
                TransactionId = ExtractKhaltiTransactionId(payment.Notes)
            };
        }

        return new CustomerPaymentListResponse
        {
            PaymentId = payment.PaymentId,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            PaymentType = payment.PaymentType.ToString(),
            InvoiceKind = "Unknown",
            TransactionId = ExtractKhaltiTransactionId(payment.Notes)
        };
    }

    private static string? ExtractKhaltiTransactionId(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return null;

        const string prefix = "khalti_txn:";
        var index = notes.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            return null;

        var start = index + prefix.Length;
        var end = notes.IndexOf(';', start);
        return end < 0 ? notes[start..].Trim() : notes[start..end].Trim();
    }
}
