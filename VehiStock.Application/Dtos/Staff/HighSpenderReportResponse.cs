namespace VehiStock.Application.Dtos.Staff;

// Used for high spender report response
public class HighSpenderReportResponse
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalPaid { get; set; }
}
