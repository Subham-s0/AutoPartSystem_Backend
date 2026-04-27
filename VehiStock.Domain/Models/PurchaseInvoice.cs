using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("PurchaseInvoices")]
public class PurchaseInvoice
{
    private decimal _subtotal;
    private decimal _taxAmount;
    private decimal _discountAmount;
    private decimal _totalAmount;

    [Key]
    public int PurchaseInvoiceId { get; set; }

    [Required]
    public int VendorId { get; set; }

    [Required]
    public string InvoiceNo { get; set; } = string.Empty;

    public DateOnly PurchaseDate { get; set; }

    public decimal Subtotal
    {
        get => _subtotal;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Subtotal), "Subtotal cannot be negative.");
            }

            _subtotal = value;
        }
    }

    public decimal TaxAmount
    {
        get => _taxAmount;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TaxAmount), "TaxAmount cannot be negative.");
            }

            _taxAmount = value;
        }
    }

    public decimal DiscountAmount
    {
        get => _discountAmount;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(DiscountAmount), "DiscountAmount cannot be negative.");
            }

            _discountAmount = value;
        }
    }

    public decimal TotalAmount
    {
        get => _totalAmount;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TotalAmount), "TotalAmount cannot be negative.");
            }

            _totalAmount = value;
        }
    }

    public PaymentStatus PaymentStatus { get; set; }

    public string? Notes { get; set; }

    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    public Vendor Vendor { get; set; } = null!;

    public ApplicationUser CreatedByUser { get; set; } = null!;

    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
}
