namespace VehiStock.Application.Dtos.Management;

public class CustomerDirectoryVehicleResponse
{
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int ManufactureYear { get; set; }
    public int MileageKm { get; set; }
}
