using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Customer;

public class CreatePartRequestRequest
{
    [Range(1, int.MaxValue)]
    public int? VehicleId { get; set; }

    [Required]
    [MaxLength(150)]
    public string RequestedPartName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [MaxLength(1000)]
    public string? Details { get; set; }
}
