namespace VehiStock.Application.Dtos.Customer;

public class UnreviewedServiceResponse
{
    public int ServiceRecordId { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public DateOnly ServiceDate { get; init; }
    public string WorkDone { get; init; } = string.Empty;
    public string Diagnosis { get; init; } = string.Empty;
}
