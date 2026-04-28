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

    public async Task<(IReadOnlyCollection<RegularCustomerReportResponse> Items, int TotalRecords)> GetRegularCustomersAsync(int pageNumber, int pageSize, int minimumInvoices, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SalesInvoices
            .AsNoTracking()
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

    public async Task<(IReadOnlyCollection<HighSpenderReportResponse> Items, int TotalRecords)> GetHighSpendersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SalesInvoices
            .AsNoTracking()
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

    public async Task<(IReadOnlyCollection<PendingCreditReportResponse> Items, int TotalRecords)> GetPendingCreditsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SalesInvoices
            .AsNoTracking()
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
}
