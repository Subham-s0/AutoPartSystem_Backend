namespace VehiStock.Application.Dtos.Customer;

public class InvoicePaymentVerifyResponse
{
    public int ServiceInvoiceId { get; init; }

    public int? SalesInvoiceId { get; init; }

    public string KhaltiStatus { get; init; } = string.Empty;

    public string MappedPaymentStatus { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string? TransactionId { get; init; }

    public bool AlreadyProcessed { get; init; }

    public decimal NewAmountPaid { get; init; }

    public decimal NewBalanceDue { get; init; }

    public string NewPaymentStatus { get; init; } = string.Empty;
}
