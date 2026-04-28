using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("StaffsProfile")]
public class StaffProfile
{
    [Key]
    public int StaffMemberId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string JobTitle { get; set; } = string.Empty;

    public DateOnly HireDate { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();

    public ICollection<Payment> ReceivedPayments { get; set; } = new List<Payment>();

    public ICollection<Appointment> AssignedAppointments { get; set; } = new List<Appointment>();

    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
}
