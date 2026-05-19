namespace VehiStock.Application.Dtos.Staff;

public class CreateServiceRecordFromAppointmentRequest
{
    public int AppointmentId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string WorkDone { get; set; } = string.Empty;
    public decimal LaborCharge { get; set; }
    public decimal PartsCharge { get; set; }
    public string? Notes { get; set; }
}
