using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("PurchaseInvoiceItems")]
public class PurchaseInvoiceItem
{
    private int _quantity;
    private decimal _unitCost;
    private decimal _lineTotal;

    [Key]
    public int PurchaseInvoiceItemId { get; set; }

    [Required]
    public int PurchaseInvoiceId { get; set; }

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

    public decimal UnitCost
    {
        get => _unitCost;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(UnitCost), "UnitCost cannot be negative.");
            }

            _unitCost = value;
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

    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;

    public Part Part { get; set; } = null!;
}
