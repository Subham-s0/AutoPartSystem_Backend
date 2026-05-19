using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.Staff},{RoleNames.Admin}")]
[Route("api/staff/dashboard")]
public class StaffDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StaffDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<StaffDashboardResponse>>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var totalActiveCustomers = await _context.CustomerProfiles
                .CountAsync(c => c.User.IsActive, cancellationToken);

            var totalPartsInCatalog = await _context.Parts
                .CountAsync(p => p.IsActive, cancellationToken);

            var lowStockPartsCount = await _context.Parts
                .CountAsync(p => p.IsActive && p.StockQty < p.MinimumStock, cancellationToken);

            var pendingServiceAppointments = await _context.Appointments
                .CountAsync(a => a.Status == AppointmentStatus.Pending, cancellationToken);

            var todayRevenue = await _context.SalesInvoices
                .Where(s => s.InvoiceDate == today)
                .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

            var todaySalesInvoiceCount = await _context.SalesInvoices
                .CountAsync(s => s.InvoiceDate == today, cancellationToken);

            var recentSalesInvoices = await _context.SalesInvoices
                .OrderByDescending(s => s.SalesInvoiceId)
                .Take(5)
                .Select(s => new RecentSalesInvoiceDto
                {
                    SalesInvoiceId = s.SalesInvoiceId,
                    InvoiceNo = s.InvoiceNo,
                    CustomerName = s.Customer.User.FullName,
                    TotalAmount = s.TotalAmount,
                    InvoiceDate = s.InvoiceDate.ToDateTime(TimeOnly.MinValue)
                })
                .ToListAsync(cancellationToken);

            var lowStockParts = await _context.Parts
                .Where(p => p.IsActive && p.StockQty < p.MinimumStock)
                .Select(p => new LowStockPartDto
                {
                    PartId = p.PartId,
                    PartName = p.PartName,
                    Brand = p.Brand,
                    StockQty = p.StockQty
                })
                .ToListAsync(cancellationToken);

            var response = new StaffDashboardResponse
            {
                TotalActiveCustomers = totalActiveCustomers,
                TotalPartsInCatalog = totalPartsInCatalog,
                LowStockPartsCount = lowStockPartsCount,
                PendingServiceAppointments = pendingServiceAppointments,
                TodayRevenue = todayRevenue,
                TodaySalesInvoiceCount = todaySalesInvoiceCount,
                RecentSalesInvoices = recentSalesInvoices,
                LowStockParts = lowStockParts
            };

            return Ok(ApiResponse<StaffDashboardResponse>.Ok(response, "Staff dashboard summary retrieved successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<StaffDashboardResponse>.Fail("An error occurred while fetching staff dashboard: " + ex.Message));
        }
    }
}

public class StaffDashboardResponse
{
    public int TotalActiveCustomers { get; set; }
    public int TotalPartsInCatalog { get; set; }
    public int LowStockPartsCount { get; set; }
    public int PendingServiceAppointments { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TodaySalesInvoiceCount { get; set; }
    public List<RecentSalesInvoiceDto> RecentSalesInvoices { get; set; } = new();
    public List<LowStockPartDto> LowStockParts { get; set; } = new();
}

public class RecentSalesInvoiceDto
{
    public int SalesInvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime InvoiceDate { get; set; }
}

public class LowStockPartDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int StockQty { get; set; }
}
