namespace VehiStock.Application.Dtos.Management;

public class StaffCustomerHistoryResponse
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public decimal TotalSpent { get; set; }
    public IReadOnlyCollection<StaffCustomerHistoryItemResponse> HistoryItems { get; set; } = [];
}
