namespace VehiStock.Application.Dtos.Staff;

// Used for sales invoice item response
public class SalesInvoiceItemResponse
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}
