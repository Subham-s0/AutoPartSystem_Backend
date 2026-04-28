using VehiStock.Application.Dtos.Reports;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Reports> GetDailyReport(DateTime date)
        {
            var sales = _context.SalesInvoices
                .Where(x => x.InvoiceDate.ToDateTime(TimeOnly.MinValue).Date == date.Date);

            var purchases = _context.PurchaseInvoices
                .Where(x => x.PurchaseDate.ToDateTime(TimeOnly.MinValue).Date == date.Date);

            return new Reports
            {
                TotalSalesRevenue = sales.Sum(x => x.TotalAmount),
                TotalPurchaseCost = purchases.Sum(x => x.TotalAmount),
                TotalProfit = sales.Sum(x => x.TotalAmount) - purchases.Sum(x => x.TotalAmount),
                TotalItemsSold = sales.SelectMany(x => x.Items).Sum(x => x.Quantity),
                FromDate = date,
                ToDate = date
            };
        }

        public async Task<Reports> GetMonthlyReport(int year, int month)
        {
            var sales = _context.SalesInvoices
                .Where(x => x.InvoiceDate.Year == year && x.InvoiceDate.Month == month);

            var purchases = _context.PurchaseInvoices
                .Where(x => x.PurchaseDate.Year == year && x.PurchaseDate.Month == month);

            return new Reports
            {
                TotalSalesRevenue = sales.Sum(x => x.TotalAmount),
                TotalPurchaseCost = purchases.Sum(x => x.TotalAmount),
                TotalProfit = sales.Sum(x => x.TotalAmount) - purchases.Sum(x => x.TotalAmount),
                TotalItemsSold = sales.SelectMany(x => x.Items).Sum(x => x.Quantity),
                FromDate = new DateTime(year, month, 1),
                ToDate = new DateTime(year, month, DateTime.DaysInMonth(year, month))
            };
        }

        public async Task<Reports> GetYearlyReport(int year)
        {
            var sales = _context.SalesInvoices
                .Where(x => x.InvoiceDate.Year == year);

            var purchases = _context.PurchaseInvoices
                .Where(x => x.PurchaseDate.Year == year);

            return new Reports
            {
                TotalSalesRevenue = sales.Sum(x => x.TotalAmount),
                TotalPurchaseCost = purchases.Sum(x => x.TotalAmount),
                TotalProfit = sales.Sum(x => x.TotalAmount) - purchases.Sum(x => x.TotalAmount),
                TotalItemsSold = sales.SelectMany(x => x.Items).Sum(x => x.Quantity),
                FromDate = new DateTime(year, 1, 1),
                ToDate = new DateTime(year, 12, 31)
            };
        }
    }
}