using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Services;

public class StaffDashboardService : IStaffDashboardService
{
    private readonly ApplicationDbContext _context;

    public StaffDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffDashboardResponse> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var totalCustomers = await _context.CustomerProfiles.CountAsync(cancellationToken);
        var totalParts = await _context.Parts.CountAsync(cancellationToken);
        
        var lowStockPartsCount = await _context.Parts
            .CountAsync(p => p.StockQty <= p.MinimumStock, cancellationToken);
            
        var pendingAppointmentsCount = await _context.Appointments
            .CountAsync(a => a.Status == AppointmentStatus.Pending, cancellationToken);
            
        var todayRevenue = await _context.SalesInvoices
            .Where(si => si.InvoiceDate == today)
            .SumAsync(si => (decimal?)si.TotalAmount, cancellationToken) ?? 0m;
            
        var totalSalesInvoicesToday = await _context.SalesInvoices
            .CountAsync(si => si.InvoiceDate == today, cancellationToken);

        var recentInvoices = await _context.SalesInvoices
            .Include(si => si.Customer)
                .ThenInclude(c => c.User)
            .OrderByDescending(si => si.SalesInvoiceId)
            .Take(5)
            .Select(si => new RecentSalesInvoiceDto
            {
                SalesInvoiceId = si.SalesInvoiceId,
                InvoiceNo = si.InvoiceNo,
                CustomerName = si.Customer.User.FullName,
                TotalAmount = si.TotalAmount,
                InvoiceDate = si.InvoiceDate
            })
            .ToListAsync(cancellationToken);

        var lowStockParts = await _context.Parts
            .Where(p => p.StockQty <= p.MinimumStock)
            .OrderBy(p => p.StockQty)
            .Take(5)
            .Select(p => new LowStockPartDto
            {
                PartId = p.PartId,
                PartName = p.PartName,
                Brand = p.Brand,
                StockQty = p.StockQty,
                MinimumStock = p.MinimumStock
            })
            .ToListAsync(cancellationToken);

        return new StaffDashboardResponse
        {
            TotalActiveCustomers = totalCustomers,
            TotalPartsInCatalog = totalParts,
            LowStockPartsCount = lowStockPartsCount,
            PendingServiceAppointments = pendingAppointmentsCount,
            TodayRevenue = todayRevenue,
            TodaySalesInvoiceCount = totalSalesInvoicesToday,
            RecentSalesInvoices = recentInvoices,
            LowStockParts = lowStockParts
        };
    }
}
