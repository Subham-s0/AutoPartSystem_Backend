using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("Payments")]
public class Payment
{
    private decimal _amount;

    [Key]
    public int PaymentId { get; set; }

    public int? SalesInvoiceId { get; set; }

    public int? ServiceInvoiceId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public PaymentType PaymentType { get; set; }

    public decimal Amount
    {
        get => _amount;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Amount), "Amount must be greater than zero.");
            }

            _amount = value;
        }
    }

    public string? Notes { get; set; }

    public SalesInvoice? SalesInvoice { get; set; }

    public ServiceInvoice? ServiceInvoice { get; set; }

    public CustomerProfile Customer { get; set; } = null!;
}
