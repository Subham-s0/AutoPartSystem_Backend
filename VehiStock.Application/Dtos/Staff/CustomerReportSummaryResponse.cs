namespace VehiStock.Application.Dtos.Staff;

public class CustomerReportSummaryResponse
{
    public int TotalCustomersWithInvoices { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalOutstandingBalance { get; set; }
    public decimal AverageCustomerSpend { get; set; }
}
