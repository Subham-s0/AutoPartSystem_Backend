namespace VehiStock.Application.Dtos.Management;

public class CustomerReportSnapshotResponse
{
    public int TotalVehicles { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateOnly? LastInvoiceDate { get; set; }
}
