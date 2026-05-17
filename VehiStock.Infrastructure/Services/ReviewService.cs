using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<PaginatedResponse<ReviewResponse>> GetReviewsPageAsync(string userId, ReviewQueryRequest query, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var normalizedQuery = new ReviewQueryRequest
        {
            PageNumber = Math.Max(1, query.PageNumber),
            PageSize = Math.Clamp(query.PageSize, 1, 50),
            SearchText = query.SearchText,
            Rating = query.Rating,
            Sorts = query.Sorts
        };

        var page = await _reviewRepository.GetReviewsPageAsync(customer.CustomerId, normalizedQuery, cancellationToken);

        return new PaginatedResponse<ReviewResponse>
        {
            Items = page.Items.Select(MapReview).ToList(),
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalRecords = page.TotalRecords,
            TotalPages = page.TotalPages
        };
    }

    public async Task<IReadOnlyCollection<UnreviewedServiceResponse>> GetUnreviewedServicesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var services = await _reviewRepository.GetUnreviewedServicesAsync(customer.CustomerId, cancellationToken);

        return services.Select(s => new UnreviewedServiceResponse
        {
            ServiceRecordId = s.ServiceRecordId,
            VehicleNumber = s.Vehicle.VehicleNumber,
            ServiceDate = s.ServiceDate,
            WorkDone = s.WorkDone,
            Diagnosis = s.Diagnosis
        }).ToList();
    }

    public async Task<ReviewResponse> CreateReviewAsync(string userId, CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var serviceRecord = await _reviewRepository.GetServiceRecordForCustomerAsync(customer.CustomerId, request.ServiceRecordId, cancellationToken);
        if (serviceRecord is null)
        {
            throw new InvalidOperationException("Service record not found for this customer.");
        }

        if (serviceRecord.Status != ServiceRecordStatus.Closed)
        {
            throw new InvalidOperationException("You can only review completed service records.");
        }

        var hasExistingReview = await _reviewRepository.HasReviewForServiceRecordAsync(customer.CustomerId, request.ServiceRecordId, cancellationToken);
        if (hasExistingReview)
        {
            throw new InvalidOperationException("A review has already been submitted for this service.");
        }

        var review = new Review
        {
            CustomerId = customer.CustomerId,
            ServiceRecordId = serviceRecord.ServiceRecordId,
            Rating = request.Rating,
            ReviewText = request.ReviewText.Trim()
        };

        var created = await _reviewRepository.CreateReviewAsync(review, cancellationToken);
        return MapReview(created);
    }

    public async Task<ReviewResponse> UpdateReviewAsync(string userId, int reviewId, UpdateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var review = await _reviewRepository.GetReviewByIdAsync(customer.CustomerId, reviewId, cancellationToken);
        if (review is null)
        {
            throw new InvalidOperationException("Review not found.");
        }

        review.Rating = request.Rating;
        review.ReviewText = request.ReviewText.Trim();

        var updated = await _reviewRepository.UpdateReviewAsync(review, cancellationToken);
        return MapReview(updated);
    }

    public async Task DeleteReviewAsync(string userId, int reviewId, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var review = await _reviewRepository.GetReviewByIdAsync(customer.CustomerId, reviewId, cancellationToken);
        if (review is null)
        {
            throw new InvalidOperationException("Review not found.");
        }

        await _reviewRepository.DeleteReviewAsync(review, cancellationToken);
    }

    private async Task<CustomerProfile> GetCustomerProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var customer = await _reviewRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer profile was not found for this account.");
        }

        return customer;
    }

    private static ReviewResponse MapReview(Review review)
    {
        return new ReviewResponse
        {
            ReviewId = review.ReviewId,
            ServiceRecordId = review.ServiceRecordId,
            VehicleNumber = review.ServiceRecord?.Vehicle?.VehicleNumber ?? string.Empty,
            ServiceDate = review.ServiceRecord?.ServiceDate ?? default,
            Diagnosis = review.ServiceRecord?.Diagnosis ?? string.Empty,
            WorkDone = review.ServiceRecord?.WorkDone ?? string.Empty,
            Rating = review.Rating,
            ReviewText = review.ReviewText,
            CreatedAt = review.CreatedAt
        };
    }
}
