using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.DTOs.Admin;

public class AdminPartRequestDto
{
    public int PartRequestId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? VehicleId { get; set; }
    public string? VehicleNumber { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
}

public class UpdatePartRequestStatusDto
{
    public string Status { get; set; } = string.Empty;
}
