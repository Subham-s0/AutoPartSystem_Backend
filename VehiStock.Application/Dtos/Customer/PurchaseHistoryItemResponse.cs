namespace VehiStock.Application.Dtos.Customer;

public class PurchaseHistoryItemResponse
{
    public string PartName { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal LineTotal { get; init; }
}
