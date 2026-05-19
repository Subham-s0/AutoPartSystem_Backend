using System;
using System.Collections.Generic;

namespace VehiStock.Application.Dtos.Staff;

public class StaffDashboardResponse
{
    public int TotalActiveCustomers { get; set; }
    public int TotalPartsInCatalog { get; set; }
    public int LowStockPartsCount { get; set; }
    public int PendingServiceAppointments { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TodaySalesInvoiceCount { get; set; }
    
    public IReadOnlyCollection<RecentSalesInvoiceDto> RecentSalesInvoices { get; set; } = Array.Empty<RecentSalesInvoiceDto>();
    public IReadOnlyCollection<LowStockPartDto> LowStockParts { get; set; } = Array.Empty<LowStockPartDto>();
}

public class RecentSalesInvoiceDto
{
    public int SalesInvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateOnly InvoiceDate { get; set; }
}

public class LowStockPartDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int StockQty { get; set; }
    public int MinimumStock { get; set; }
}
