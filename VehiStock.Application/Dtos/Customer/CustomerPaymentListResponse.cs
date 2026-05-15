namespace VehiStock.Application.Dtos.Customer;

public class CustomerPaymentListResponse
{
    public int PaymentId { get; init; }

    public DateTime PaymentDate { get; init; }

    public decimal Amount { get; init; }

    public string PaymentType { get; init; } = string.Empty;

    public string InvoiceKind { get; init; } = string.Empty;

    public int? SalesInvoiceId { get; init; }

    public int? ServiceInvoiceId { get; init; }

    public string InvoiceReference { get; init; } = string.Empty;

    public string VehicleNumber { get; init; } = string.Empty;

    public string InvoicePaymentStatus { get; init; } = string.Empty;

    public string? TransactionId { get; init; }
}
