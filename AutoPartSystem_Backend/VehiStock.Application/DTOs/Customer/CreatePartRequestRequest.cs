using System.ComponentModel.DataAnnotations;
using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Dtos.Customer;

public class CreatePartRequestRequest
{
    [Range(1, int.MaxValue)]
    public int? VehicleId { get; set; }

    [Required]
    [MaxLength(150)]
    public string RequestedPartName { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    [MaxLength(1000)]
    public string? Details { get; set; }

    public ImageUploadFile? Photo { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }
}
