namespace VehiStock.Application.DTOs.Reports;

public class CustomerReportFilterDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int TopCount { get; set; } = 10;
    public int MinimumInvoices { get; set; } = 2;
}
