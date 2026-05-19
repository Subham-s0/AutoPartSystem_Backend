using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("Reviews")]
public class Review
{
    private int _rating;

    [Key]
    public int ReviewId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int ServiceRecordId { get; set; }

    [Required]
    public int Rating
    {
        get => _rating;
        set
        {
            if (value < 1 || value > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(Rating), "Rating must be between 1 and 5.");
            }

            _rating = value;
        }
    }

    [Required]
    public string ReviewText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CustomerProfile Customer { get; set; } = null!;

    public ServiceRecord ServiceRecord { get; set; } = null!;
}
