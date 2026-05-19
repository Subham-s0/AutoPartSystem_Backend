namespace VehiStock.Application.Dtos.Customer;

public class CustomerHistoryResponse
{
    public IReadOnlyCollection<PurchaseHistoryResponse> Purchases { get; init; } = Array.Empty<PurchaseHistoryResponse>();
    public IReadOnlyCollection<ServiceHistoryResponse> Services { get; init; } = Array.Empty<ServiceHistoryResponse>();
}
