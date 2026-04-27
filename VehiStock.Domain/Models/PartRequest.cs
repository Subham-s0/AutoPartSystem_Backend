using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("PartRequests")]
public class PartRequest
{
    [Key]
    public int PartRequestId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public int? VehicleId { get; set; }

    [Required]
    public string RequestedPartName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string? Details { get; set; }

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    public PartRequestStatus Status { get; set; }

    public CustomerProfile Customer { get; set; } = null!;

    public Vehicle? Vehicle { get; set; }
}
