namespace VehiStock.Application.DTOs
{
    public class CreatePurchaseInvoiceItemDto
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
    }
}