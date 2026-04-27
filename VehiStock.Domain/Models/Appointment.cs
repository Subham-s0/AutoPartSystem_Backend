using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("Appointments")]
public class Appointment
{
    [Key]
    public int AppointmentId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    public DateOnly PreferredDate { get; set; }

    [Required]
    public string ServiceType { get; set; } = string.Empty;

    [Required]
    public string ProblemDescription { get; set; } = string.Empty;

    public AppointmentStatus Status { get; set; }

    public int? AssignedStaffId { get; set; }

    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    public CustomerProfile Customer { get; set; } = null!;

    public Vehicle Vehicle { get; set; } = null!;

    public StaffProfile? AssignedStaff { get; set; }

    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
}
