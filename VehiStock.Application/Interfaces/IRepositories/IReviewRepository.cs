using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IReviewRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<ServiceRecord?> GetServiceRecordForCustomerAsync(int customerId, int serviceRecordId, CancellationToken cancellationToken = default);
    Task<bool> HasReviewForServiceRecordAsync(int customerId, int serviceRecordId, CancellationToken cancellationToken = default);
    Task<Review> CreateReviewAsync(Review review, CancellationToken cancellationToken = default);
    Task<Review?> GetReviewByIdAsync(int customerId, int reviewId, CancellationToken cancellationToken = default);
    Task<Review> UpdateReviewAsync(Review review, CancellationToken cancellationToken = default);
    Task DeleteReviewAsync(Review review, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Review>> GetReviewsPageAsync(int customerId, ReviewQueryRequest query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ServiceRecord>> GetUnreviewedServicesAsync(int customerId, CancellationToken cancellationToken = default);
}
