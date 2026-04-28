namespace VehiStock.Application.Dtos.Customer;

public class PurchaseHistoryResponse
{
    public int SalesInvoiceId { get; init; }
    public string InvoiceNo { get; init; } = string.Empty;
    public DateOnly InvoiceDate { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal BalanceDue { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public IReadOnlyCollection<PurchaseHistoryItemResponse> Items { get; init; } = Array.Empty<PurchaseHistoryItemResponse>();
}
