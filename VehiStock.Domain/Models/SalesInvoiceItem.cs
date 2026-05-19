using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("SalesInvoiceItems")]
public class SalesInvoiceItem
{
    private int _quantity;
    private decimal _unitPrice;
    private decimal _discountAmount;
    private decimal _lineTotal;

    [Key]
    public int SalesInvoiceItemId { get; set; }

    [Required]
    public int SalesInvoiceId { get; set; }

    [Required]
    public int PartId { get; set; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Quantity), "Quantity must be greater than zero.");
            }

            _quantity = value;
        }
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(UnitPrice), "UnitPrice cannot be negative.");
            }

            _unitPrice = value;
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

    public decimal LineTotal
    {
        get => _lineTotal;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(LineTotal), "LineTotal cannot be negative.");
            }

            _lineTotal = value;
        }
    }

    public SalesInvoice SalesInvoice { get; set; } = null!;

    public Part Part { get; set; } = null!;
}
