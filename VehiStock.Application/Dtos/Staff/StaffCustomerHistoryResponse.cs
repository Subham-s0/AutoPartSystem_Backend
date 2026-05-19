namespace VehiStock.Application.Dtos.Staff;

public class StaffCustomerHistoryResponse
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    
    public List<StaffCustomerHistoryItem> HistoryItems { get; set; } = new();
}

public class StaffCustomerHistoryItem
{
    public string Type { get; set; } = string.Empty; 
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}
