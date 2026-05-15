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
[Route("api/customer/part-requests")]
public class CustomerPartRequestsController : ControllerBase
{
    private readonly ICustomerPartRequestService _partRequestService;

    public CustomerPartRequestsController(ICustomerPartRequestService partRequestService)
    {
        _partRequestService = partRequestService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PartRequestResponse>>>> GetPartRequests(
        [FromQuery] PartRequestQueryRequest query,
        CancellationToken cancellationToken)
    {
        try
        {
            var partRequests = await _partRequestService.GetPartRequestsPageAsync(
                GetCurrentUserId(),
                query,
                cancellationToken);
            return Ok(ApiResponse<PaginatedResponse<PartRequestResponse>>.Ok(partRequests, "Part requests fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaginatedResponse<PartRequestResponse>>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PartRequestResponse>>> CreatePartRequest(
        CreatePartRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var partRequest = await _partRequestService.CreatePartRequestAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<PartRequestResponse>.Ok(partRequest, "Part request submitted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PartRequestResponse>.Fail(ex.Message));
        }
    }

    [HttpPatch("{partRequestId:int}/cancel")]
    public async Task<ActionResult<ApiResponse<PartRequestResponse>>> CancelPartRequest(
        int partRequestId,
        CancellationToken cancellationToken)
    {
        try
        {
            var partRequest = await _partRequestService.CancelPartRequestAsync(
                GetCurrentUserId(),
                partRequestId,
                cancellationToken);
            return Ok(ApiResponse<PartRequestResponse>.Ok(partRequest, "Part request cancelled."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PartRequestResponse>.Fail(ex.Message));
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
