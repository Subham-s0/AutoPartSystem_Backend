namespace VehiStock.Application.Dtos.Admin;

public class AdminPartRequestResponse
{
    public int PartRequestId { get; init; }
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public int? VehicleId { get; init; }
    public string? VehicleNumber { get; init; }
    public string? VehicleMake { get; init; }
    public string? VehicleModel { get; init; }
    public int? VehicleManufactureYear { get; init; }
    public string? VehiclePhotoUrl { get; init; }
    public string RequestedPartName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string? Details { get; init; }
    public string? PhotoUrl { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime RequestDate { get; init; }
}
