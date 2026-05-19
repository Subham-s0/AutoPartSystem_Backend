using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Customer;

public class CreatePartRequestFormRequest
{
    public int? VehicleId { get; set; }

    [Required]
    [MaxLength(150)]
    public string RequestedPartName { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    [MaxLength(1000)]
    public string? Details { get; set; }

    public IFormFile? Photo { get; set; }

    [MaxLength(500)]
    [Url]
    public string? PhotoUrl { get; set; }
}
