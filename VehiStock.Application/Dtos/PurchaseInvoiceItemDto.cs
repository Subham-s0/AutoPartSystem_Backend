namespace VehiStock.Application.DTOs
{
    public class PurchaseInvoiceItemDto
    {
        public int PurchaseInvoiceItemId { get; set; }
        public int PartId { get; set; }
        public string? PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
    }
}