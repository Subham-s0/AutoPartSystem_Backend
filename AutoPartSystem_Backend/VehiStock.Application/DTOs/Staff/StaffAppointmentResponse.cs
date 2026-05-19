using System;

namespace VehiStock.Application.Dtos.Staff;

public class StaffAppointmentResponse
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public DateOnly PreferredDate { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedStaffId { get; set; }
    public string? AssignedStaffName { get; set; }
    public DateTime BookedAt { get; set; }
}
