using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class CustomerServiceInvoiceRepository : ICustomerServiceInvoiceRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerServiceInvoiceRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResponse<ServiceInvoice>> GetServiceInvoicesPageAsync(
        int customerId,
        ServiceInvoiceQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = BuildCustomerInvoiceQuery()
            .Where(x => x.CustomerId == customerId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim().ToLower();
            query = query.Where(x =>
                x.Vehicle.VehicleNumber.ToLower().Contains(searchText) ||
                x.ServiceRecord.Diagnosis.ToLower().Contains(searchText));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<PaymentStatus>(request.Status.Trim(), true, out var status))
        {
            query = query.Where(x => x.PaymentStatus == status);
        }

        query = ApplyServiceInvoiceSorting(query, request.Sorts);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<ServiceInvoice>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)request.PageSize)
        };
    }

    public Task<ServiceInvoice?> GetServiceInvoiceForCustomerAsync(int customerId, int serviceInvoiceId, CancellationToken cancellationToken = default)
    {
        return BuildCustomerInvoiceQuery()
            .FirstOrDefaultAsync(
                x => x.CustomerId == customerId && x.ServiceInvoiceId == serviceInvoiceId,
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ServiceInvoice> BuildCustomerInvoiceQuery()
    {
        return _dbContext.ServiceInvoices
            .Include(x => x.Vehicle)
            .Include(x => x.ServiceRecord)
                .ThenInclude(x => x.StaffMember)
                    .ThenInclude(x => x.User)
            .Include(x => x.ServiceRecord)
                .ThenInclude(x => x.PartsUsed)
                    .ThenInclude(x => x.Part);
    }

    private static IQueryable<ServiceInvoice> ApplyServiceInvoiceSorting(IQueryable<ServiceInvoice> query, List<SortRequest> sorts)
    {
        if (sorts.Count == 0)
        {
            return query
                .OrderByDescending(x => x.ServiceRecord.ServiceDate)
                .ThenByDescending(x => x.ServiceInvoiceId);
        }

        IOrderedQueryable<ServiceInvoice>? ordered = null;
        foreach (var sort in sorts)
        {
            var asc = sort.SortDirection == SortDirection.Asc;
            ordered = sort.SortBy.Trim().ToLowerInvariant() switch
            {
                "totalamount" or "amount" => ordered is null
                    ? asc ? query.OrderBy(x => x.TotalAmount) : query.OrderByDescending(x => x.TotalAmount)
                    : asc ? ordered.ThenBy(x => x.TotalAmount) : ordered.ThenByDescending(x => x.TotalAmount),
                "servicedate" or "date" => ordered is null
                    ? asc ? query.OrderBy(x => x.ServiceRecord.ServiceDate) : query.OrderByDescending(x => x.ServiceRecord.ServiceDate)
                    : asc ? ordered.ThenBy(x => x.ServiceRecord.ServiceDate) : ordered.ThenByDescending(x => x.ServiceRecord.ServiceDate),
                _ => ordered is null
                    ? asc ? query.OrderBy(x => x.ServiceRecord.ServiceDate) : query.OrderByDescending(x => x.ServiceRecord.ServiceDate)
                    : asc ? ordered.ThenBy(x => x.ServiceRecord.ServiceDate) : ordered.ThenByDescending(x => x.ServiceRecord.ServiceDate),
            };
        }

        return ordered!.ThenByDescending(x => x.ServiceInvoiceId);
    }
}
