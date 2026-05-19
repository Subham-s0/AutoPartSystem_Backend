using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;
using static VehiStock.Application.Dtos.Common.SortDirection;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class AdminPartRequestRepository : IAdminPartRequestRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AdminPartRequestRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResponse<PartRequest>> GetPartRequestsPageAsync(
        AdminPartRequestQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _dbContext.PartRequests
            .Include(x => x.Vehicle)
            .Include(x => x.Customer).ThenInclude(c => c.User)
            .AsQueryable();

        if (query.CustomerId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.CustomerId == query.CustomerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var term = query.SearchText.Trim().ToLower();
            baseQuery = baseQuery.Where(x =>
                x.RequestedPartName.ToLower().Contains(term) ||
                (x.Vehicle != null && x.Vehicle.VehicleNumber.ToLower().Contains(term)) ||
                (x.Details != null && x.Details.ToLower().Contains(term)) ||
                x.Customer.User.FullName.ToLower().Contains(term) ||
                x.Customer.User.Email!.ToLower().Contains(term) ||
                (x.Customer.User.PhoneNumber != null && x.Customer.User.PhoneNumber.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<PartRequestStatus>(query.Status.Trim(), true, out var status))
        {
            baseQuery = baseQuery.Where(x => x.Status == status);
        }

        baseQuery = ApplyPartRequestSorting(baseQuery, query.Sorts);

        var totalRecords = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<PartRequest>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)query.PageSize)
        };
    }

    public Task<PartRequest?> GetPartRequestByIdAsync(
        int partRequestId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.PartRequests
            .Include(x => x.Vehicle)
            .Include(x => x.Customer).ThenInclude(c => c.User)
            .SingleOrDefaultAsync(x => x.PartRequestId == partRequestId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<PartRequest> ApplyPartRequestSorting(IQueryable<PartRequest> query, List<SortRequest> sorts)
    {
        if (sorts.Count == 0)
        {
            return query.OrderByDescending(x => x.RequestDate).ThenByDescending(x => x.PartRequestId);
        }

        IOrderedQueryable<PartRequest>? ordered = null;

        foreach (var sort in sorts)
        {
            var asc = sort.SortDirection == Asc;

            ordered = sort.SortBy.Trim().ToLowerInvariant() switch
            {
                "status" => ordered is null
                    ? asc ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status)
                    : asc ? ordered.ThenBy(x => x.Status) : ordered.ThenByDescending(x => x.Status),
                _ => ordered is null
                    ? asc ? query.OrderBy(x => x.RequestDate) : query.OrderByDescending(x => x.RequestDate)
                    : asc ? ordered.ThenBy(x => x.RequestDate) : ordered.ThenByDescending(x => x.RequestDate),
            };
        }

        return ordered!;
    }
}
