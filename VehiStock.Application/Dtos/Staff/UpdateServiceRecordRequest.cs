namespace VehiStock.Application.Dtos.Staff;

public class UpdateServiceRecordRequest
{
    public string Diagnosis { get; init; } = string.Empty;
    public string WorkDone { get; init; } = string.Empty;
    public decimal LaborCharge { get; init; }
    public decimal PartsCharge { get; init; }
    public string? Notes { get; init; }
}
