namespace VehiStock.Application.Dtos.Staff;

public class SalesInvoiceVehicleLookupResponse
{
    public int VehicleId { get; set; }

    public string VehicleNumber { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;
}
