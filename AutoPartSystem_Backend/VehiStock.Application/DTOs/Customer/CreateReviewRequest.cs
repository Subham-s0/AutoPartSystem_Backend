using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Customer;

public class CreateReviewRequest
{
    [Range(1, int.MaxValue)]
    public int ServiceRecordId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(1500)]
    public string ReviewText { get; set; } = string.Empty;
}
