using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VehiStock.Application.Dtos.Staff;

public class SellPartRequest
{
    public int CustomerId { get; set; }

    public int PartId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [JsonPropertyName("vehicleID")]
    public int VehicleId { get; set; }
}
