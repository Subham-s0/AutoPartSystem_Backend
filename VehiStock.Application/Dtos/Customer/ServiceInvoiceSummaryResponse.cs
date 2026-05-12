namespace VehiStock.Application.Dtos.Customer;

public class ServiceInvoiceSummaryResponse
{
    public int ServiceInvoiceId { get; init; }

    public decimal LaborCharge { get; init; }

    public decimal PartsCharge { get; init; }

    public decimal TaxAmount { get; init; }

    public decimal TotalAmount { get; init; }

    public decimal AmountPaid { get; init; }

    public decimal BalanceDue { get; init; }

    public string PaymentStatus { get; init; } = string.Empty;
}
