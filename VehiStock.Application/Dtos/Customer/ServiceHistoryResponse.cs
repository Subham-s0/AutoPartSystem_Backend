namespace VehiStock.Application.Dtos.Customer;

public class ServiceHistoryResponse
{
    public int ServiceRecordId { get; init; }
    public DateOnly ServiceDate { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public string Diagnosis { get; init; } = string.Empty;
    public string WorkDone { get; init; } = string.Empty;
    public decimal LaborCharge { get; init; }
    public decimal PartsCharge { get; init; }
    public decimal TotalCharge { get; init; }
    public string? Notes { get; init; }
    public ServiceInvoiceSummaryResponse? ServiceInvoice { get; init; }
    public IReadOnlyCollection<ServiceHistoryPartResponse> PartsUsed { get; init; } = Array.Empty<ServiceHistoryPartResponse>();
    public ReviewResponse? Review { get; init; }
}
