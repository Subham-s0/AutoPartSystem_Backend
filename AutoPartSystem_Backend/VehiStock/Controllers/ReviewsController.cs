using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Customer)]
[Route("api/customer/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ReviewResponse>>>> GetReviews(
        [FromQuery] ReviewQueryRequest query,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _reviewService.GetReviewsPageAsync(GetCurrentUserId(), query, cancellationToken);
            return Ok(ApiResponse<PaginatedResponse<ReviewResponse>>.Ok(result, "Reviews fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaginatedResponse<ReviewResponse>>.Fail(ex.Message));
        }
    }

    [HttpGet("unreviewed")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UnreviewedServiceResponse>>>> GetUnreviewedServices(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _reviewService.GetUnreviewedServicesAsync(GetCurrentUserId(), cancellationToken);
            return Ok(ApiResponse<IReadOnlyCollection<UnreviewedServiceResponse>>.Ok(result, "Unreviewed services fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IReadOnlyCollection<UnreviewedServiceResponse>>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReview(
        CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var review = await _reviewService.CreateReviewAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<ReviewResponse>.Ok(review, "Review submitted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ReviewResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("{reviewId:int}")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateReview(
        int reviewId,
        UpdateReviewRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var review = await _reviewService.UpdateReviewAsync(GetCurrentUserId(), reviewId, request, cancellationToken);
            return Ok(ApiResponse<ReviewResponse>.Ok(review, "Review updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ReviewResponse>.Fail(ex.Message));
        }
    }

    [HttpDelete("{reviewId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteReview(
        int reviewId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _reviewService.DeleteReviewAsync(GetCurrentUserId(), reviewId, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null!, "Review deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Authenticated user ID is missing.");
        }

        return userId;
    }
}
