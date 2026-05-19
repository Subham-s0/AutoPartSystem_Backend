namespace VehiStock.Application.Dtos.Customer;

public class ReviewResponse
{
    public int ReviewId { get; init; }
    public int ServiceRecordId { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public DateOnly ServiceDate { get; init; }
    public string Diagnosis { get; init; } = string.Empty;
    public string WorkDone { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string ReviewText { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
