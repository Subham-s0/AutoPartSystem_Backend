using System.ComponentModel.DataAnnotations;
using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Dtos.Customer;

public class UpdateVehicleRequest : IValidatableObject
{
    [Required]
    [MaxLength(50)]
    public string VehicleNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int ManufactureYear { get; set; }

    [MaxLength(100)]
    public string? EngineNo { get; set; }

    [MaxLength(100)]
    public string? ChassisNo { get; set; }

    public ImageUploadFile? VehiclePhoto { get; set; }

    public bool RemoveVehiclePhoto { get; set; }

    [Range(0, int.MaxValue)]
    public int MileageKm { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ManufactureYear == default)
        {
            yield return new ValidationResult(
                "ManufactureYear is required.",
                [nameof(ManufactureYear)]);
        }
    }
}
