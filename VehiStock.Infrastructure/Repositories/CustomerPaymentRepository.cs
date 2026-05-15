using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class CustomerPaymentRepository : ICustomerPaymentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerPaymentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<PaginatedResponse<Payment>> GetPaymentsPageAsync(
        int customerId,
        CustomerPaymentQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Payments
            .Include(x => x.SalesInvoice)
                .ThenInclude(x => x!.Vehicle)
            .Include(x => x.ServiceInvoice)
                .ThenInclude(x => x!.Vehicle)
            .Include(x => x.ServiceInvoice)
                .ThenInclude(x => x!.ServiceRecord)
            .Where(x => x.CustomerId == customerId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.InvoiceKind))
        {
            var invoiceKind = request.InvoiceKind.Trim();
            if (string.Equals(invoiceKind, "Service", StringComparison.OrdinalIgnoreCase))
                query = query.Where(x => x.ServiceInvoiceId != null);
            else if (string.Equals(invoiceKind, "Sales", StringComparison.OrdinalIgnoreCase))
                query = query.Where(x => x.SalesInvoiceId != null);
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentType) &&
            Enum.TryParse<PaymentType>(request.PaymentType.Trim(), true, out var paymentType))
        {
            query = query.Where(x => x.PaymentType == paymentType);
        }

        if (!string.IsNullOrWhiteSpace(request.InvoiceStatus) &&
            Enum.TryParse<PaymentStatus>(request.InvoiceStatus.Trim(), true, out var invoiceStatus))
        {
            query = query.Where(x =>
                (x.SalesInvoice != null && x.SalesInvoice.PaymentStatus == invoiceStatus) ||
                (x.ServiceInvoice != null && x.ServiceInvoice.PaymentStatus == invoiceStatus));
        }

        if (request.FromDate.HasValue)
        {
            var fromUtc = request.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.PaymentDate >= fromUtc);
        }

        if (request.ToDate.HasValue)
        {
            var toUtc = request.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(x => x.PaymentDate <= toUtc);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim().ToLower();
            query = query.Where(x =>
                (x.Notes != null && x.Notes.ToLower().Contains(searchText)) ||
                (x.SalesInvoice != null &&
                 (x.SalesInvoice.InvoiceNo.ToLower().Contains(searchText) ||
                  x.SalesInvoice.Vehicle.VehicleNumber.ToLower().Contains(searchText))) ||
                (x.ServiceInvoice != null &&
                 (x.ServiceInvoice.Vehicle.VehicleNumber.ToLower().Contains(searchText) ||
                  x.ServiceInvoice.ServiceRecordId.ToString().Contains(searchText))));
        }

        query = ApplyPaymentSorting(query, request.Sorts);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<Payment>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)request.PageSize)
        };
    }

    public Task<bool> PaymentExistsForKhaltiTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return Task.FromResult(false);

        var marker = $"khalti_txn:{transactionId}";
        return _dbContext.Payments.AnyAsync(x => x.Notes != null && x.Notes.Contains(marker), cancellationToken);
    }

    public Task<ServiceInvoice?> GetServiceInvoiceForCustomerAsync(int customerId, int serviceInvoiceId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceInvoices
            .Include(x => x.Vehicle)
            .Include(x => x.ServiceRecord)
                .ThenInclude(x => x.StaffMember)
            .FirstOrDefaultAsync(
                x => x.CustomerId == customerId && x.ServiceInvoiceId == serviceInvoiceId,
                cancellationToken);
    }

    public Task<SalesInvoice?> GetSalesInvoiceForCustomerAsync(int customerId, int salesInvoiceId, CancellationToken cancellationToken = default)
    {
        return _dbContext.SalesInvoices
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(
                x => x.CustomerId == customerId && x.SalesInvoiceId == salesInvoiceId,
                cancellationToken);
    }

    public async Task AddPaymentAndSaveAsync(Payment payment, ServiceInvoice serviceInvoice, CancellationToken cancellationToken = default)
    {
        _dbContext.Payments.Add(payment);
        _dbContext.ServiceInvoices.Update(serviceInvoice);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddSalesInvoicePaymentAndSaveAsync(Payment payment, SalesInvoice salesInvoice, CancellationToken cancellationToken = default)
    {
        _dbContext.Payments.Add(payment);
        _dbContext.SalesInvoices.Update(salesInvoice);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Payment> ApplyPaymentSorting(IQueryable<Payment> query, List<SortRequest> sorts)
    {
        if (sorts.Count == 0)
        {
            return query
                .OrderByDescending(x => x.PaymentDate)
                .ThenByDescending(x => x.PaymentId);
        }

        IOrderedQueryable<Payment>? ordered = null;
        foreach (var sort in sorts)
        {
            var asc = sort.SortDirection == SortDirection.Asc;
            ordered = sort.SortBy.Trim().ToLowerInvariant() switch
            {
                "amount" => ordered is null
                    ? asc ? query.OrderBy(x => x.Amount) : query.OrderByDescending(x => x.Amount)
                    : asc ? ordered.ThenBy(x => x.Amount) : ordered.ThenByDescending(x => x.Amount),
                "paymentdate" or "date" => ordered is null
                    ? asc ? query.OrderBy(x => x.PaymentDate) : query.OrderByDescending(x => x.PaymentDate)
                    : asc ? ordered.ThenBy(x => x.PaymentDate) : ordered.ThenByDescending(x => x.PaymentDate),
                _ => ordered is null
                    ? asc ? query.OrderBy(x => x.PaymentDate) : query.OrderByDescending(x => x.PaymentDate)
                    : asc ? ordered.ThenBy(x => x.PaymentDate) : ordered.ThenByDescending(x => x.PaymentDate),
            };
        }

        return ordered!.ThenByDescending(x => x.PaymentId);
    }
}
