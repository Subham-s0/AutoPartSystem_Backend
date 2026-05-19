namespace VehiStock.Application.Dtos.Management;

public class StaffCustomerHistoryItemResponse
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}
