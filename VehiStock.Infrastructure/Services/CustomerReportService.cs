using Microsoft.EntityFrameworkCore;
using VehiStock.Application.DTOs.Reports;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Services;

public class CustomerReportService(ApplicationDbContext dbContext) : ICustomerReportService
{
    public async Task<IReadOnlyList<RegularCustomerReportItemDto>> GetRegularCustomersAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var topCount = NormalizeTopCount(filter.TopCount);
        var minimumInvoices = filter.MinimumInvoices <= 0 ? 2 : filter.MinimumInvoices;

        var query = FilterInvoices(dbContext.SalesInvoices.AsNoTracking(), filter)
            .Where(x => x.Customer.User != null)
            .GroupBy(x => new
            {
                x.CustomerId,
                x.Customer.CustomerCode,
                x.Customer.User.FullName,
                x.Customer.User.Email
            })
            .Select(group => new RegularCustomerReportItemDto
            {
                CustomerId = group.Key.CustomerId,
                CustomerCode = group.Key.CustomerCode,
                FullName = group.Key.FullName,
                Email = group.Key.Email ?? string.Empty,
                InvoiceCount = group.Count(),
                TotalSpent = group.Sum(x => x.TotalAmount),
                LastInvoiceDate = group.Max(x => (DateOnly?)x.InvoiceDate)
            })
            .Where(x => x.InvoiceCount >= minimumInvoices)
            .OrderByDescending(x => x.InvoiceCount)
            .ThenByDescending(x => x.TotalSpent)
            .Take(topCount);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HighSpenderReportItemDto>> GetHighSpendersAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var topCount = NormalizeTopCount(filter.TopCount);

        var query = FilterInvoices(dbContext.SalesInvoices.AsNoTracking(), filter)
            .Where(x => x.Customer.User != null)
            .GroupBy(x => new
            {
                x.CustomerId,
                x.Customer.CustomerCode,
                x.Customer.User.FullName,
                x.Customer.User.Email
            })
            .Select(group => new HighSpenderReportItemDto
            {
                CustomerId = group.Key.CustomerId,
                CustomerCode = group.Key.CustomerCode,
                FullName = group.Key.FullName,
                Email = group.Key.Email ?? string.Empty,
                InvoiceCount = group.Count(),
                TotalSpent = group.Sum(x => x.TotalAmount),
                TotalPaid = group.Sum(x => x.AmountPaid)
            })
            .OrderByDescending(x => x.TotalSpent)
            .ThenByDescending(x => x.TotalPaid)
            .Take(topCount);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PendingCreditReportItemDto>> GetPendingCreditsAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = FilterInvoices(dbContext.SalesInvoices.AsNoTracking(), filter)
            .Where(x => x.BalanceDue > 0m && (x.PaymentStatus == PaymentStatus.Unpaid || x.PaymentStatus == PaymentStatus.Partial || x.PaymentStatus == PaymentStatus.Overdue))
            .Select(x => new PendingCreditReportItemDto
            {
                SalesInvoiceId = x.SalesInvoiceId,
                InvoiceNo = x.InvoiceNo,
                CustomerId = x.CustomerId,
                CustomerCode = x.Customer.CustomerCode,
                FullName = x.Customer.User!.FullName,
                Email = x.Customer.User!.Email ?? string.Empty,
                InvoiceDate = x.InvoiceDate,
                CreditDueDate = x.CreditDueDate,
                TotalAmount = x.TotalAmount,
                AmountPaid = x.AmountPaid,
                BalanceDue = x.BalanceDue
            })
            .OrderByDescending(x => x.BalanceDue)
            .ThenBy(x => x.CreditDueDate);

        return await query.ToListAsync(cancellationToken);
    }

    private static IQueryable<SalesInvoice> FilterInvoices(IQueryable<SalesInvoice> query, CustomerReportFilterDto filter)
    {
        if (filter.FromDate.HasValue)
        {
            query = query.Where(x => x.InvoiceDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(x => x.InvoiceDate <= filter.ToDate.Value);
        }

        return query;
    }

    private static int NormalizeTopCount(int topCount) => topCount <= 0 ? 10 : topCount;
}
