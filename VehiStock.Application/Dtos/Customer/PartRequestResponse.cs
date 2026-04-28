namespace VehiStock.Application.Dtos.Customer;

public class PartRequestResponse
{
    public int PartRequestId { get; init; }
    public int? VehicleId { get; init; }
    public string? VehicleNumber { get; init; }
    public string RequestedPartName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string? Details { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime RequestDate { get; init; }
}
