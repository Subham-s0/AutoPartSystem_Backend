namespace VehiStock.Application.DTOs.Reports;

public class RegularCustomerReportItemDto
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateOnly? LastInvoiceDate { get; set; }
}
