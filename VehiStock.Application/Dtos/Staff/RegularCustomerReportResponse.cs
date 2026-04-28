namespace VehiStock.Application.Dtos.Staff;

// Used for regular customer report response
public class RegularCustomerReportResponse
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateOnly? LastInvoiceDate { get; set; }
}
