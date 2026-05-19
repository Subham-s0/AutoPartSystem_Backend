namespace VehiStock.Application.Dtos.Reports
{
    public class Reports
    {
        public decimal TotalSalesRevenue { get; set; }

        public decimal TotalServiceRevenue { get; set; }

        public decimal TotalCombinedRevenue { get; set; }

        public decimal TotalPurchaseCost { get; set; }

        public decimal TotalProfit { get; set; }

        public int TotalItemsSold { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public List<ReportBreakdown> Breakdown { get; set; } = new();
    }
}