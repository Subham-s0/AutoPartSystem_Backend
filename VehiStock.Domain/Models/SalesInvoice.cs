using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("SalesInvoices")]
public class SalesInvoice
{
    private decimal _subtotal;
    private decimal _discountPercent;
    private decimal _discountAmount;
    private decimal _taxAmount;
    private decimal _totalAmount;
    private decimal _amountPaid;
    private decimal _balanceDue;

    [Key]
    public int SalesInvoiceId { get; set; }

    [Required]
    public string InvoiceNo { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    [Required]
    public int StaffMemberId { get; set; }

    public DateOnly InvoiceDate { get; set; }

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

    public decimal DiscountPercent
    {
        get => _discountPercent;
        set
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(DiscountPercent), "DiscountPercent must be between 0 and 100.");
            }

            _discountPercent = value;
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

    public decimal AmountPaid
    {
        get => _amountPaid;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(AmountPaid), "AmountPaid cannot be negative.");
            }

            _amountPaid = value;
        }
    }

    public decimal BalanceDue
    {
        get => _balanceDue;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(BalanceDue), "BalanceDue cannot be negative.");
            }

            _balanceDue = value;
        }
    }

    public DateOnly? CreditDueDate { get; set; }

    public PaymentType PaymentType { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public DateTime? EmailSentAt { get; set; }

    public CustomerProfile Customer { get; set; } = null!;

    public Vehicle Vehicle { get; set; } = null!;

    public StaffProfile StaffMember { get; set; } = null!;

    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
