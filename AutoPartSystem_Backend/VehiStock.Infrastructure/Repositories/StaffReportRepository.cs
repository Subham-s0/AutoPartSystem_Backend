using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

// Implementation for staff report data access
public class StaffReportRepository : IStaffReportRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StaffReportRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyCollection<RegularCustomerReportResponse> Items, int TotalRecords)> GetRegularCustomersAsync(int pageNumber, int pageSize, int minimumInvoices, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        var query = ApplyDateRange(_dbContext.SalesInvoices.AsNoTracking(), fromDate, toDate)
            .GroupBy(x => new
            {
                x.CustomerId,
                x.Customer.User.FullName,
                x.Customer.User.Email
            })
            .Select(group => new RegularCustomerReportResponse
            {
                CustomerId = group.Key.CustomerId,
                FullName = group.Key.FullName,
                Email = group.Key.Email ?? string.Empty,
                InvoiceCount = group.Count(),
                TotalSpent = group.Sum(x => x.TotalAmount),
                LastInvoiceDate = group.Max(x => (DateOnly?)x.InvoiceDate)
            })
            .Where(x => x.InvoiceCount >= minimumInvoices)
            .OrderByDescending(x => x.InvoiceCount)
            .ThenByDescending(x => x.TotalSpent);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalRecords);
    }

    public async Task<(IReadOnlyCollection<HighSpenderReportResponse> Items, int TotalRecords)> GetHighSpendersAsync(int pageNumber, int pageSize, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        var query = ApplyDateRange(_dbContext.SalesInvoices.AsNoTracking(), fromDate, toDate)
            .GroupBy(x => new
            {
                x.CustomerId,
                x.Customer.User.FullName,
                x.Customer.User.Email
            })
            .Select(group => new HighSpenderReportResponse
            {
                CustomerId = group.Key.CustomerId,
                FullName = group.Key.FullName,
                Email = group.Key.Email ?? string.Empty,
                InvoiceCount = group.Count(),
                TotalSpent = group.Sum(x => x.TotalAmount),
                TotalPaid = group.Sum(x => x.AmountPaid)
            })
            .OrderByDescending(x => x.TotalSpent)
            .ThenByDescending(x => x.TotalPaid);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalRecords);
    }

    public async Task<(IReadOnlyCollection<PendingCreditReportResponse> Items, int TotalRecords)> GetPendingCreditsAsync(int pageNumber, int pageSize, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        var query = ApplyDateRange(_dbContext.SalesInvoices.AsNoTracking(), fromDate, toDate)
            .Where(x => x.BalanceDue > 0m)
            .Select(x => new PendingCreditReportResponse
            {
                SalesInvoiceId = x.SalesInvoiceId,
                InvoiceNo = x.InvoiceNo,
                CustomerId = x.CustomerId,
                FullName = x.Customer.User.FullName,
                Email = x.Customer.User.Email ?? string.Empty,
                InvoiceDate = x.InvoiceDate,
                CreditDueDate = x.CreditDueDate,
                TotalAmount = x.TotalAmount,
                AmountPaid = x.AmountPaid,
                BalanceDue = x.BalanceDue
            })
            .OrderByDescending(x => x.BalanceDue)
            .ThenBy(x => x.CreditDueDate);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalRecords);
    }

    public async Task<CustomerReportSummaryResponse> GetSummaryAsync(DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        var query = ApplyDateRange(_dbContext.SalesInvoices.AsNoTracking(), fromDate, toDate);

        var totalCustomersWithInvoices = await query
            .Select(x => x.CustomerId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalInvoices = await query.CountAsync(cancellationToken);
        var totalRevenue = await query.SumAsync(x => (decimal?)x.TotalAmount, cancellationToken) ?? 0m;
        var totalOutstandingBalance = await query.SumAsync(x => (decimal?)x.BalanceDue, cancellationToken) ?? 0m;
        var averageCustomerSpend = totalCustomersWithInvoices == 0 ? 0m : totalRevenue / totalCustomersWithInvoices;

        return new CustomerReportSummaryResponse
        {
            TotalCustomersWithInvoices = totalCustomersWithInvoices,
            TotalInvoices = totalInvoices,
            TotalRevenue = totalRevenue,
            TotalOutstandingBalance = totalOutstandingBalance,
            AverageCustomerSpend = Math.Round(averageCustomerSpend, 2, MidpointRounding.AwayFromZero)
        };
    }

    private static IQueryable<VehiStock.Entities.SalesInvoice> ApplyDateRange(IQueryable<VehiStock.Entities.SalesInvoice> query, DateOnly? fromDate, DateOnly? toDate)
    {
        if (fromDate.HasValue)
        {
            query = query.Where(x => x.InvoiceDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.InvoiceDate <= toDate.Value);
        }

        return query;
    }
}
