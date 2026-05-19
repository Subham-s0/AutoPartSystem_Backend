namespace VehiStock.Application.Dtos.Reports
{
    public class ReportBreakdown
    {
        public string Label { get; set; } = string.Empty;
        public decimal SalesRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit => Revenue - Cost;
    }
}
