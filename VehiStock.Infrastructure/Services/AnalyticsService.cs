using Microsoft.EntityFrameworkCore;
using VehiStock.Application.DTOs.Analytics;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var totalVendors = await _context.Vendors.CountAsync();

            var totalParts = await _context.Parts.CountAsync();

            var totalInvoices = await _context.PurchaseInvoices.CountAsync();

            var lowStockParts = await _context.Parts
                .Where(p => p.StockQty <= p.MinimumStock)
                .CountAsync();

            var totalPurchaseAmount = await _context.PurchaseInvoices
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            var purchaseInvoicesList = await _context.PurchaseInvoices
                .Select(i => new { i.PurchaseDate, i.TotalAmount })
                .ToListAsync();

            var monthlyPurchases = purchaseInvoicesList
                .GroupBy(i => i.PurchaseDate.Month)
                .Select(g => new MonthlyPurchaseDto
                {
                    Month = $"Month {g.Key}",
                    Amount = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToList();

            var lowStockItems = await _context.Parts
                .Where(p => p.StockQty <= p.MinimumStock)
                .Select(p => new LowStockPartAnalyticsDto
                {
                    PartName = p.PartName,
                    StockQty = p.StockQty
                })
                .ToListAsync();

            var recentActivities = new List<RecentActivityAnalyticsDto>
            {
                new RecentActivityAnalyticsDto
                {
                    Activity = "Vendor records updated",
                    Module = "Vendor",
                    Status = "Completed"
                },

                new RecentActivityAnalyticsDto
                {
                    Activity = "Stock quantities updated",
                    Module = "Inventory",
                    Status = "Completed"
                },

                new RecentActivityAnalyticsDto
                {
                    Activity = "Purchase invoice generated",
                    Module = "Invoice",
                    Status = "Completed"
                }
            };

            return new DashboardSummaryDto
            {
                TotalVendors = totalVendors,
                TotalParts = totalParts,
                TotalPurchaseInvoices = totalInvoices,
                LowStockParts = lowStockParts,
                TotalPurchaseAmount = totalPurchaseAmount,
                MonthlyPurchases = monthlyPurchases,
                LowStockItems = lowStockItems,
                RecentActivities = recentActivities
            };
        }
    }
}
