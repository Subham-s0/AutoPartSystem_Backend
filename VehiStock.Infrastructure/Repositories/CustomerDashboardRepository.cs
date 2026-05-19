using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class CustomerDashboardRepository : ICustomerDashboardRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerDashboardRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> GetCustomerIdByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.CustomerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Customer profile not found.");

        return customer.CustomerId;
    }

    public Task<int> GetActiveVehiclesCountAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Vehicles
            .CountAsync(v => v.CustomerId == customerId, cancellationToken);
    }

    public Task<int> GetUpcomingAppointmentsCountAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _dbContext.Appointments
            .CountAsync(a => a.CustomerId == customerId
                && a.Status == AppointmentStatus.Confirmed
                && a.PreferredDate > today, cancellationToken);
    }

    public async Task<decimal> GetOutstandingBalanceAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var salesBalance = await _dbContext.SalesInvoices
            .Where(si => si.CustomerId == customerId
                && si.BalanceDue > 0m
                && si.PaymentStatus != PaymentStatus.Paid
                && si.PaymentStatus != PaymentStatus.Cancelled)
            .SumAsync(si => (decimal?)si.BalanceDue, cancellationToken) ?? 0m;

        var serviceBalance = await _dbContext.ServiceInvoices
            .Where(si => si.CustomerId == customerId
                && si.BalanceDue > 0m
                && si.PaymentStatus != PaymentStatus.Paid
                && si.PaymentStatus != PaymentStatus.Cancelled)
            .SumAsync(si => (decimal?)si.BalanceDue, cancellationToken) ?? 0m;

        return salesBalance + serviceBalance;
    }

    public Task<int> GetPendingPartRequestsCountAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PartRequests
            .CountAsync(pr => pr.CustomerId == customerId
                && (pr.Status == PartRequestStatus.Pending || pr.Status == PartRequestStatus.Ordered), cancellationToken);
    }

    public async Task<IReadOnlyCollection<MonthlySpendingDto>> GetSpendingTrendAsync(
        int customerId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
        var startDateOnly = DateOnly.FromDateTime(startDate);

        var salesByMonth = await _dbContext.SalesInvoices
            .Where(si => si.CustomerId == customerId && si.InvoiceDate >= startDateOnly)
            .GroupBy(si => new { si.InvoiceDate.Year, si.InvoiceDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(si => si.TotalAmount),
            })
            .ToListAsync(cancellationToken);

        var serviceByMonth = await _dbContext.ServiceInvoices
            .Include(si => si.ServiceRecord)
            .Where(si => si.CustomerId == customerId && si.ServiceRecord.ServiceDate >= startDateOnly)
            .GroupBy(si => new { si.ServiceRecord.ServiceDate.Year, si.ServiceRecord.ServiceDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(si => si.TotalAmount),
            })
            .ToListAsync(cancellationToken);

        var result = new List<MonthlySpendingDto>();
        for (var i = 0; i < 12; i++)
        {
            var target = startDate.AddMonths(i);
            var year = target.Year;
            var month = target.Month;

            var salesTotal = salesByMonth
                .Where(s => s.Year == year && s.Month == month)
                .Sum(s => s.Total);

            var serviceTotal = serviceByMonth
                .Where(s => s.Year == year && s.Month == month)
                .Sum(s => s.Total);

            result.Add(new MonthlySpendingDto
            {
                Month = target.ToString("MMM", CultureInfo.InvariantCulture),
                Amount = salesTotal + serviceTotal,
            });
        }

        return result;
    }

    public async Task<IReadOnlyCollection<RecentActivityDto>> GetRecentActivitiesAsync(
        int customerId, CancellationToken cancellationToken = default)
    {
        var activities = new List<RecentActivityDto>();

        var recentPayments = await _dbContext.Payments
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .Take(3)
            .Select(p => new RecentActivityDto
            {
                Type = "Payment",
                Description = p.SalesInvoiceId != null
                    ? "Payment on sales invoice"
                    : "Payment on service invoice",
                Date = p.PaymentDate,
            })
            .ToListAsync(cancellationToken);
        activities.AddRange(recentPayments);

        var recentAppointments = await _dbContext.Appointments
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.BookedAt)
            .Take(3)
            .Include(a => a.Vehicle)
            .Select(a => new RecentActivityDto
            {
                Type = "Appointment",
                Description = $"{a.ServiceType} for {a.Vehicle.Make} {a.Vehicle.Model}",
                Date = a.BookedAt,
            })
            .ToListAsync(cancellationToken);
        activities.AddRange(recentAppointments);

        var recentServices = await _dbContext.ServiceRecords
            .Where(sr => sr.CustomerId == customerId)
            .OrderByDescending(sr => sr.ServiceDate)
            .Take(3)
            .Include(sr => sr.Vehicle)
            .Select(sr => new RecentActivityDto
            {
                Type = "Service",
                Description = $"{sr.WorkDone} - {sr.Vehicle.Make} {sr.Vehicle.Model}",
                Date = sr.ServiceDate.ToDateTime(TimeOnly.MinValue),
            })
            .ToListAsync(cancellationToken);
        activities.AddRange(recentServices);

        return activities
            .OrderByDescending(a => a.Date)
            .Take(5)
            .ToList();
    }
}
