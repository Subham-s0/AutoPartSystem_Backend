using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("Parts")]
public class Part
{
    private decimal _unitCost;
    private decimal _unitPrice;
    private int _stockQty;
    private int _minimumStock;

    [Key]
    public int PartId { get; set; }

    [Required]
    public int PartCategoryId { get; set; }

    [Required]
    public string PartName { get; set; } = string.Empty;

    [Required]
    public string Brand { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? PartPhotoUrl { get; set; }

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

    public int StockQty
    {
        get => _stockQty;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(StockQty), "StockQty cannot be negative.");
            }

            _stockQty = value;
        }
    }

    public int MinimumStock
    {
        get => _minimumStock;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(MinimumStock), "MinimumStock cannot be negative.");
            }

            _minimumStock = value;
        }
    }

    public bool IsActive { get; set; } = true;

    public bool IsLowStock => StockQty < MinimumStock;

    public PartCategory PartCategory { get; set; } = null!;

    public ICollection<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; } = new List<PurchaseInvoiceItem>();

    public ICollection<SalesInvoiceItem> SalesInvoiceItems { get; set; } = new List<SalesInvoiceItem>();

    public ICollection<ServiceRecordPart> ServiceRecordParts { get; set; } = new List<ServiceRecordPart>();

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "quantity must be greater than zero.");
        }

        StockQty += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "quantity must be greater than zero.");
        }

        if (StockQty - quantity < 0)
        {
            throw new InvalidOperationException("Stock cannot go below zero.");
        }

        StockQty -= quantity;
    }
}
