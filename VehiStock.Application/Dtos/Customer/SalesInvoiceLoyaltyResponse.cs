namespace VehiStock.Application.Dtos.Customer;

public class SalesInvoiceLoyaltyResponse
{
    public int SalesInvoiceId { get; init; }
    public string InvoiceNo { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal BalanceDue { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
}
