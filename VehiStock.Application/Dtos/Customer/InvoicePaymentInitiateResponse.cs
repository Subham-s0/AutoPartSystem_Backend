namespace VehiStock.Application.Dtos.Customer;

public class InvoicePaymentInitiateResponse
{
    public int ServiceInvoiceId { get; init; }

    public int? SalesInvoiceId { get; init; }

    public int ServiceRecordId { get; init; }

    public string Pidx { get; init; } = string.Empty;

    public string PaymentUrl { get; init; } = string.Empty;

    public DateTime? ExpiresAt { get; init; }

    public decimal Amount { get; init; }
}
