using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("ServiceInvoices")]
public class ServiceInvoice
{
    private decimal _laborCharge;
    private decimal _partsCharge;
    private decimal _taxAmount;
    private decimal _totalAmount;
    private decimal _amountPaid;
    private decimal _balanceDue;

    [Key]
    public int ServiceInvoiceId { get; set; }

    [Required]
    public int ServiceRecordId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    public decimal LaborCharge
    {
        get => _laborCharge;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(LaborCharge), "LaborCharge cannot be negative.");
            }

            _laborCharge = value;
        }
    }

    public decimal PartsCharge
    {
        get => _partsCharge;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(PartsCharge), "PartsCharge cannot be negative.");
            }

            _partsCharge = value;
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

    public PaymentStatus PaymentStatus { get; set; }

    public ServiceRecord ServiceRecord { get; set; } = null!;

    public CustomerProfile Customer { get; set; } = null!;

    public Vehicle Vehicle { get; set; } = null!;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
