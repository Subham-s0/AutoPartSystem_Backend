using VehiStock.Entities;

namespace VehiStock.Application.DTOs
{
    public class CreatePurchaseInvoiceDto
    {
        public int VendorId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateOnly PurchaseDate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? Notes { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;

        public List<CreatePurchaseInvoiceItemDto> Items { get; set; } = new();
    }
}