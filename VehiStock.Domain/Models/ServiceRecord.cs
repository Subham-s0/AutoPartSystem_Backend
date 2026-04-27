using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("ServiceRecords")]
public class ServiceRecord
{
    private decimal _laborCharge;
    private decimal _partsCharge;
    private decimal _totalCharge;

    [Key]
    public int ServiceRecordId { get; set; }

    public int? AppointmentId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    [Required]
    public int StaffMemberId { get; set; }

    public DateOnly ServiceDate { get; set; }

    [Required]
    public string Diagnosis { get; set; } = string.Empty;

    [Required]
    public string WorkDone { get; set; } = string.Empty;

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

    public decimal TotalCharge
    {
        get => _totalCharge;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TotalCharge), "TotalCharge cannot be negative.");
            }

            _totalCharge = value;
        }
    }

    public string? Notes { get; set; }

    public Appointment? Appointment { get; set; }

    public CustomerProfile Customer { get; set; } = null!;

    public Vehicle Vehicle { get; set; } = null!;

    public StaffProfile StaffMember { get; set; } = null!;

    public ICollection<ServiceRecordPart> PartsUsed { get; set; } = new List<ServiceRecordPart>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
