namespace VehiStock.Application.DTOs.Analytics
{
    public class DashboardSummaryDto
    {
        public int TotalVendors { get; set; }

        public int TotalParts { get; set; }

        public int TotalPurchaseInvoices { get; set; }

        public int LowStockParts { get; set; }

        public decimal TotalPurchaseAmount { get; set; }

        public List<MonthlyPurchaseDto> MonthlyPurchases { get; set; } = new();

        public List<LowStockPartDto> LowStockItems { get; set; } = new();

        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }

    public class MonthlyPurchaseDto
    {
        public string Month { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }

    public class LowStockPartDto
    {
        public string PartName { get; set; } = string.Empty;

        public int StockQty { get; set; }
    }

    public class RecentActivityDto
    {
        public string Activity { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}