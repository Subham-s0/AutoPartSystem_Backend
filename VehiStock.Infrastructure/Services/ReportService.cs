using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            var targetDate = DateOnly.FromDateTime(date);

            var sales = _context.SalesInvoices
                .Where(x => x.InvoiceDate == targetDate);

            var services = _context.ServiceInvoices
                .Include(x => x.ServiceRecord)
                .Where(x => x.ServiceRecord.ServiceDate == targetDate);

            var purchases = _context.PurchaseInvoices
                .Where(x => x.PurchaseDate == targetDate);

            var salesList = await sales.ToListAsync();
            var servicesList = await services.ToListAsync();
            var purchasesList = await purchases.ToListAsync();

            var salesRevenue = salesList.Sum(x => x.TotalAmount);
            var serviceRevenue = servicesList.Sum(x => x.TotalAmount);
            var totalRevenue = salesRevenue + serviceRevenue;
            var totalCost = purchasesList.Sum(x => x.TotalAmount);

            var breakdown = new List<ReportBreakdown>
            {
                new ReportBreakdown
                {
                    Label = date.ToString("MMM dd, yyyy"),
                    SalesRevenue = salesRevenue,
                    ServiceRevenue = serviceRevenue,
                    Revenue = totalRevenue,
                    Cost = totalCost
                }
            };

            var salesItemsCount = await sales.SelectMany(x => x.Items).SumAsync(x => (int?)x.Quantity) ?? 0;
            var serviceItemsCount = await services.SelectMany(x => x.ServiceRecord.PartsUsed).SumAsync(x => (int?)x.Quantity) ?? 0;

            return new Reports
            {
                TotalSalesRevenue = salesRevenue,
                TotalServiceRevenue = serviceRevenue,
                TotalCombinedRevenue = totalRevenue,
                TotalPurchaseCost = totalCost,
                TotalProfit = totalRevenue - totalCost,
                TotalItemsSold = salesItemsCount + serviceItemsCount,
                FromDate = date,
                ToDate = date,
                Breakdown = breakdown
            };
        }

        public async Task<Reports> GetMonthlyReport(int year, int month)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            var sales = _context.SalesInvoices
                .Where(x => x.InvoiceDate >= startDate && x.InvoiceDate <= endDate);

            var services = _context.ServiceInvoices
                .Include(x => x.ServiceRecord)
                .Where(x => x.ServiceRecord.ServiceDate >= startDate && x.ServiceRecord.ServiceDate <= endDate);

            var purchases = _context.PurchaseInvoices
                .Where(x => x.PurchaseDate >= startDate && x.PurchaseDate <= endDate);

            var salesList = await sales.ToListAsync();
            var servicesList = await services.ToListAsync();
            var purchasesList = await purchases.ToListAsync();

            var breakdown = new List<ReportBreakdown>();
            for (int i = 1; i <= DateTime.DaysInMonth(year, month); i++)
            {
                var currentDay = new DateOnly(year, month, i);
                var dailySales = salesList.Where(x => x.InvoiceDate == currentDay).Sum(x => x.TotalAmount);
                var dailyServices = servicesList.Where(x => x.ServiceRecord.ServiceDate == currentDay).Sum(x => x.TotalAmount);
                var dailyPurchases = purchasesList.Where(x => x.PurchaseDate == currentDay).Sum(x => x.TotalAmount);
                
                breakdown.Add(new ReportBreakdown
                {
                    Label = $"Day {i}",
                    SalesRevenue = dailySales,
                    ServiceRevenue = dailyServices,
                    Revenue = dailySales + dailyServices,
                    Cost = dailyPurchases
                });
            }

            var salesRevenue = salesList.Sum(x => x.TotalAmount);
            var serviceRevenue = servicesList.Sum(x => x.TotalAmount);
            var totalRevenue = salesRevenue + serviceRevenue;
            var totalCost = purchasesList.Sum(x => x.TotalAmount);

            var salesItemsCount = await sales.SelectMany(x => x.Items).SumAsync(x => (int?)x.Quantity) ?? 0;
            var serviceItemsCount = await services.SelectMany(x => x.ServiceRecord.PartsUsed).SumAsync(x => (int?)x.Quantity) ?? 0;

            return new Reports
            {
                TotalSalesRevenue = salesRevenue,
                TotalServiceRevenue = serviceRevenue,
                TotalCombinedRevenue = totalRevenue,
                TotalPurchaseCost = totalCost,
                TotalProfit = totalRevenue - totalCost,
                TotalItemsSold = salesItemsCount + serviceItemsCount,
                FromDate = new DateTime(year, month, 1),
                ToDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                Breakdown = breakdown
            };
        }

        public async Task<Reports> GetYearlyReport(int year)
        {
            var startDate = new DateOnly(year, 1, 1);
            var endDate = new DateOnly(year, 12, 31);

            var sales = _context.SalesInvoices
                .Where(x => x.InvoiceDate >= startDate && x.InvoiceDate <= endDate);

            var services = _context.ServiceInvoices
                .Include(x => x.ServiceRecord)
                .Where(x => x.ServiceRecord.ServiceDate >= startDate && x.ServiceRecord.ServiceDate <= endDate);

            var purchases = _context.PurchaseInvoices
                .Where(x => x.PurchaseDate >= startDate && x.PurchaseDate <= endDate);

            var salesList = await sales.ToListAsync();
            var servicesList = await services.ToListAsync();
            var purchasesList = await purchases.ToListAsync();

            var breakdown = new List<ReportBreakdown>();
            for (int i = 1; i <= 12; i++)
            {
                var monthlySales = salesList.Where(x => x.InvoiceDate.Month == i).Sum(x => x.TotalAmount);
                var monthlyServices = servicesList.Where(x => x.ServiceRecord.ServiceDate.Month == i).Sum(x => x.TotalAmount);
                var monthlyPurchases = purchasesList.Where(x => x.PurchaseDate.Month == i).Sum(x => x.TotalAmount);
                
                breakdown.Add(new ReportBreakdown
                {
                    Label = new DateTime(year, i, 1).ToString("MMM"),
                    SalesRevenue = monthlySales,
                    ServiceRevenue = monthlyServices,
                    Revenue = monthlySales + monthlyServices,
                    Cost = monthlyPurchases
                });
            }

            var salesRevenue = salesList.Sum(x => x.TotalAmount);
            var serviceRevenue = servicesList.Sum(x => x.TotalAmount);
            var totalRevenue = salesRevenue + serviceRevenue;
            var totalCost = purchasesList.Sum(x => x.TotalAmount);

            var salesItemsCount = await sales.SelectMany(x => x.Items).SumAsync(x => (int?)x.Quantity) ?? 0;
            var serviceItemsCount = await services.SelectMany(x => x.ServiceRecord.PartsUsed).SumAsync(x => (int?)x.Quantity) ?? 0;

            return new Reports
            {
                TotalSalesRevenue = salesRevenue,
                TotalServiceRevenue = serviceRevenue,
                TotalCombinedRevenue = totalRevenue,
                TotalPurchaseCost = totalCost,
                TotalProfit = totalRevenue - totalCost,
                TotalItemsSold = salesItemsCount + serviceItemsCount,
                FromDate = new DateTime(year, 1, 1),
                ToDate = new DateTime(year, 12, 31),
                Breakdown = breakdown
            };
        }
    }
}