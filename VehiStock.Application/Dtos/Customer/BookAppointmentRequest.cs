using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Customer;

public class BookAppointmentRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    public DateOnly PreferredDate { get; set; }

    [Required]
    [MaxLength(100)]
    public string ServiceType { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string ProblemDescription { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PreferredDate == default)
        {
            yield return new ValidationResult(
                "PreferredDate is required.",
                [nameof(PreferredDate)]);
        }
    }
}
