using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class CustomerHistoryRepository : ICustomerHistoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerHistoryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResponse<SalesInvoice>> GetPurchaseHistoryPageAsync(
        int customerId,
        PurchaseHistoryQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SalesInvoices
            .Include(x => x.Vehicle)
            .Include(x => x.Items)
                .ThenInclude(x => x.Part)
            .Where(x => x.CustomerId == customerId && x.Items.Any())
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim().ToLower();
            query = query.Where(x =>
                x.Vehicle.VehicleNumber.ToLower().Contains(searchText) ||
                x.InvoiceNo.ToLower().Contains(searchText) ||
                x.Items.Any(item =>
                    item.Part.PartName.ToLower().Contains(searchText) ||
                    item.Part.Brand.ToLower().Contains(searchText)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<PaymentStatus>(request.Status.Trim(), true, out var status))
        {
            query = query.Where(x => x.PaymentStatus == status);
        }

        query = ApplyPurchaseHistorySorting(query, request.Sorts);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<SalesInvoice>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)request.PageSize)
        };
    }

    public async Task<IReadOnlyCollection<SalesInvoice>> GetPurchaseHistoryAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SalesInvoices
            .Include(x => x.Vehicle)
            .Include(x => x.Items)
                .ThenInclude(x => x.Part)
            .Where(x => x.CustomerId == customerId && x.Items.Any())
            .OrderByDescending(x => x.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResponse<ServiceRecord>> GetServiceHistoryPageAsync(
        int customerId,
        ServiceHistoryQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ServiceRecords
            .Include(x => x.Vehicle)
            .Include(x => x.StaffMember)
                .ThenInclude(x => x.User)
            .Include(x => x.PartsUsed)
                .ThenInclude(x => x.Part)
            .Include(x => x.ServiceInvoice)
            .Include(x => x.Reviews)
            .Where(x => x.CustomerId == customerId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim().ToLower();
            query = query.Where(x =>
                x.Vehicle.VehicleNumber.ToLower().Contains(searchText) ||
                x.Diagnosis.ToLower().Contains(searchText));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<ServiceRecordStatus>(request.Status.Trim(), true, out var status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.InvoiceStatus))
        {
            var invoiceStatusToken = request.InvoiceStatus.Trim();
            if (string.Equals(invoiceStatusToken, "NotInvoiced", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.ServiceInvoice == null);
            }
            else if (Enum.TryParse<PaymentStatus>(invoiceStatusToken, true, out var invoiceStatus))
            {
                query = query.Where(x => x.ServiceInvoice != null && x.ServiceInvoice.PaymentStatus == invoiceStatus);
            }
        }

        query = ApplyServiceHistorySorting(query, request.Sorts);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .AsSplitQuery()
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<ServiceRecord>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)request.PageSize)
        };
    }

    public Task<ServiceRecord?> GetServiceRecordDetailAsync(int customerId, int serviceRecordId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceRecords
            .Include(x => x.Vehicle)
            .Include(x => x.StaffMember)
                .ThenInclude(x => x.User)
            .Include(x => x.PartsUsed)
                .ThenInclude(x => x.Part)
            .Include(x => x.ServiceInvoice)
            .Include(x => x.Reviews)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                x => x.CustomerId == customerId && x.ServiceRecordId == serviceRecordId,
                cancellationToken);
    }

    private static IQueryable<SalesInvoice> ApplyPurchaseHistorySorting(IQueryable<SalesInvoice> query, List<SortRequest> sorts)
    {
        if (sorts.Count == 0)
        {
            return query
                .OrderByDescending(x => x.InvoiceDate)
                .ThenByDescending(x => x.SalesInvoiceId);
        }

        IOrderedQueryable<SalesInvoice>? ordered = null;
        foreach (var sort in sorts)
        {
            var asc = sort.SortDirection == SortDirection.Asc;
            ordered = sort.SortBy.Trim().ToLowerInvariant() switch
            {
                "totalamount" or "amount" => ordered is null
                    ? asc ? query.OrderBy(x => x.TotalAmount) : query.OrderByDescending(x => x.TotalAmount)
                    : asc ? ordered.ThenBy(x => x.TotalAmount) : ordered.ThenByDescending(x => x.TotalAmount),
                "invoicedate" or "date" => ordered is null
                    ? asc ? query.OrderBy(x => x.InvoiceDate) : query.OrderByDescending(x => x.InvoiceDate)
                    : asc ? ordered.ThenBy(x => x.InvoiceDate) : ordered.ThenByDescending(x => x.InvoiceDate),
                _ => ordered is null
                    ? asc ? query.OrderBy(x => x.InvoiceDate) : query.OrderByDescending(x => x.InvoiceDate)
                    : asc ? ordered.ThenBy(x => x.InvoiceDate) : ordered.ThenByDescending(x => x.InvoiceDate),
            };
        }

        return ordered!.ThenByDescending(x => x.SalesInvoiceId);
    }

    private static IQueryable<ServiceRecord> ApplyServiceHistorySorting(IQueryable<ServiceRecord> query, List<SortRequest> sorts)
    {
        if (sorts.Count == 0)
        {
            return query
                .OrderByDescending(x => x.ServiceDate)
                .ThenByDescending(x => x.ServiceRecordId);
        }

        IOrderedQueryable<ServiceRecord>? ordered = null;
        foreach (var sort in sorts)
        {
            var asc = sort.SortDirection == SortDirection.Asc;
            ordered = sort.SortBy.Trim().ToLowerInvariant() switch
            {
                "totalcharge" or "amount" => ordered is null
                    ? asc ? query.OrderBy(x => x.TotalCharge) : query.OrderByDescending(x => x.TotalCharge)
                    : asc ? ordered.ThenBy(x => x.TotalCharge) : ordered.ThenByDescending(x => x.TotalCharge),
                "servicedate" or "date" => ordered is null
                    ? asc ? query.OrderBy(x => x.ServiceDate) : query.OrderByDescending(x => x.ServiceDate)
                    : asc ? ordered.ThenBy(x => x.ServiceDate) : ordered.ThenByDescending(x => x.ServiceDate),
                _ => ordered is null
                    ? asc ? query.OrderBy(x => x.ServiceDate) : query.OrderByDescending(x => x.ServiceDate)
                    : asc ? ordered.ThenBy(x => x.ServiceDate) : ordered.ThenByDescending(x => x.ServiceDate),
            };
        }

        return ordered!.ThenByDescending(x => x.ServiceRecordId);
    }
}
