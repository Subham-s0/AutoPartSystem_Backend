using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface IReviewService
{
    Task<PaginatedResponse<ReviewResponse>> GetReviewsPageAsync(string userId, ReviewQueryRequest query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UnreviewedServiceResponse>> GetUnreviewedServicesAsync(string userId, CancellationToken cancellationToken = default);
    Task<ReviewResponse> CreateReviewAsync(string userId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<ReviewResponse> UpdateReviewAsync(string userId, int reviewId, UpdateReviewRequest request, CancellationToken cancellationToken = default);
    Task DeleteReviewAsync(string userId, int reviewId, CancellationToken cancellationToken = default);
}
