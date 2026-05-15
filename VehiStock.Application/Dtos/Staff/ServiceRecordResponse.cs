namespace VehiStock.Application.Dtos.Staff;

public class ServiceRecordResponse
{
    public int ServiceRecordId { get; init; }
    public int CustomerId { get; init; }
    public int VehicleId { get; init; }
    public int StaffMemberId { get; init; }
    public int? AppointmentId { get; init; }
    public DateOnly ServiceDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Diagnosis { get; init; } = string.Empty;
    public string WorkDone { get; init; } = string.Empty;
    public decimal LaborCharge { get; init; }
    public decimal PartsCharge { get; init; }
    public decimal TotalCharge { get; init; }
    public string? Notes { get; init; }
}
