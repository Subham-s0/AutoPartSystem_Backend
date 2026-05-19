namespace VehiStock.Application.Dtos.Staff;

public class SalesInvoicePartLookupResponse
{
    public int PartId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int StockQty { get; set; }
}
