using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("ServiceRecordParts")]
public class ServiceRecordPart
{
    private int _quantity;
    private decimal _unitPrice;
    private decimal _lineTotal;

    [Key]
    public int ServiceRecordPartId { get; set; }

    [Required]
    public int ServiceRecordId { get; set; }

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

    public ServiceRecord ServiceRecord { get; set; } = null!;

    public Part Part { get; set; } = null!;
}
