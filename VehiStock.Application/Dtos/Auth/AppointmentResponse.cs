namespace VehiStock.Application.Dtos.Customer;

public class AppointmentResponse
{
    public int AppointmentId { get; init; }
    public int VehicleId { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public DateOnly PreferredDate { get; init; }
    public string ServiceType { get; init; } = string.Empty;
    public string ProblemDescription { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime BookedAt { get; init; }
}
