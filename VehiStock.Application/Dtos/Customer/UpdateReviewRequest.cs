using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Customer;

public class UpdateReviewRequest
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(1500)]
    public string ReviewText { get; set; } = string.Empty;
}
