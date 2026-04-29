namespace VehiStock.Application.Dtos.Customer;

public class CustomerVehicleResponse
{
    public int VehicleId { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int ManufactureYear { get; init; }
    public int MileageKm { get; init; }
    public string? VehiclePhotoUrl { get; init; }
}
