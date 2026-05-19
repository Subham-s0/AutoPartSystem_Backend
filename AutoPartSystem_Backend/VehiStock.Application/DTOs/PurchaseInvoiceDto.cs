using VehiStock.Entities;

namespace VehiStock.Application.DTOs
{
    public class PurchaseInvoiceDto
    {
        public int PurchaseInvoiceId { get; set; }
        public int VendorId { get; set; }
        public string? VendorName { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateOnly PurchaseDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? Notes { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;

        public List<PurchaseInvoiceItemDto> Items { get; set; } = new();
    }
}