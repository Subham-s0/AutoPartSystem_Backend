using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("Vehicles")]
public class Vehicle
{
    [Key]
    public int VehicleId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public string VehicleNumber { get; set; } = string.Empty;

    [Required]
    public string Make { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    public int ManufactureYear { get; set; }

    [Required]
    public string EngineNo { get; set; } = string.Empty;

    [Required]
    public string ChassisNo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? VehiclePhotoUrl { get; set; }

    public int MileageKm { get; set; }

    public string? Notes { get; set; }

    public CustomerProfile Customer { get; set; } = null!;

    public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();

    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
}
