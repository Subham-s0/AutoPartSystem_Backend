namespace VehiStock.Application.Dtos.Customer;

public class ReviewResponse
{
    public int ReviewId { get; init; }
    public int ServiceRecordId { get; init; }
    public int Rating { get; init; }
    public string ReviewText { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
