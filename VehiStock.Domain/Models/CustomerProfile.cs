using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("CustomersProfile")]
public class CustomerProfile
{
    [Key]
    public int CustomerId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    public RegistrationSource RegistrationSource { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();

    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
