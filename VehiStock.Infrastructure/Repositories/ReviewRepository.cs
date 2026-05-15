using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using static VehiStock.Application.Dtos.Common.SortDirection;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ReviewRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<ServiceRecord?> GetServiceRecordForCustomerAsync(int customerId, int serviceRecordId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceRecords
            .Include(x => x.Vehicle)
            .Include(x => x.Reviews)
            .SingleOrDefaultAsync(x => x.CustomerId == customerId && x.ServiceRecordId == serviceRecordId, cancellationToken);
    }

    public Task<bool> HasReviewForServiceRecordAsync(int customerId, int serviceRecordId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Reviews.AnyAsync(
            x => x.CustomerId == customerId && x.ServiceRecordId == serviceRecordId,
            cancellationToken);
    }

    public async Task<Review> CreateReviewAsync(Review review, CancellationToken cancellationToken = default)
    {
        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Reviews
            .Include(x => x.ServiceRecord)
                .ThenInclude(x => x.Vehicle)
            .SingleAsync(x => x.ReviewId == review.ReviewId, cancellationToken);
    }

    public Task<Review?> GetReviewByIdAsync(int customerId, int reviewId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Reviews
            .Include(x => x.ServiceRecord)
                .ThenInclude(x => x.Vehicle)
            .SingleOrDefaultAsync(x => x.CustomerId == customerId && x.ReviewId == reviewId, cancellationToken);
    }

    public async Task<Review> UpdateReviewAsync(Review review, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return review;
    }

    public async Task DeleteReviewAsync(Review review, CancellationToken cancellationToken = default)
    {
        _dbContext.Reviews.Remove(review);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedResponse<Review>> GetReviewsPageAsync(int customerId, ReviewQueryRequest query, CancellationToken cancellationToken = default)
    {
        var baseQuery = _dbContext.Reviews
            .Include(x => x.ServiceRecord)
                .ThenInclude(x => x.Vehicle)
            .Where(x => x.CustomerId == customerId);

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var term = query.SearchText.Trim().ToLower();
            baseQuery = baseQuery.Where(x =>
                x.ReviewText.ToLower().Contains(term) ||
                x.ServiceRecord.Vehicle.VehicleNumber.ToLower().Contains(term) ||
                x.ServiceRecord.Diagnosis.ToLower().Contains(term) ||
                x.ServiceRecord.WorkDone.ToLower().Contains(term));
        }

        if (query.Rating is int rating && rating >= 1 && rating <= 5)
        {
            baseQuery = baseQuery.Where(x => x.Rating == rating);
        }

        baseQuery = ApplySorting(baseQuery, query.Sorts);

        var totalRecords = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<Review>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)query.PageSize)
        };
    }

    public async Task<IReadOnlyCollection<ServiceRecord>> GetUnreviewedServicesAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRecords
            .Include(x => x.Vehicle)
            .Where(x =>
                x.CustomerId == customerId &&
                x.Status == ServiceRecordStatus.Closed &&
                !x.Reviews.Any())
            .OrderByDescending(x => x.ServiceDate)
            .ThenByDescending(x => x.ServiceRecordId)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Review> ApplySorting(IQueryable<Review> query, List<SortRequest> sorts)
    {
        if (sorts.Count == 0)
        {
            return query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.ReviewId);
        }

        IOrderedQueryable<Review>? ordered = null;

        foreach (var sort in sorts)
        {
            var asc = sort.SortDirection == Asc;

            ordered = sort.SortBy.Trim().ToLowerInvariant() switch
            {
                "rating" => ordered is null
                    ? asc ? query.OrderBy(x => x.Rating) : query.OrderByDescending(x => x.Rating)
                    : asc ? ordered.ThenBy(x => x.Rating) : ordered.ThenByDescending(x => x.Rating),
                _ => ordered is null
                    ? asc ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt)
                    : asc ? ordered.ThenBy(x => x.CreatedAt) : ordered.ThenByDescending(x => x.CreatedAt),
            };
        }

        return ordered!;
    }
}
