using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
[Route("api/admin/part-requests")]
public class AdminPartRequestsController : ControllerBase
{
    private readonly IAdminPartRequestService _partRequestService;

    public AdminPartRequestsController(IAdminPartRequestService partRequestService)
    {
        _partRequestService = partRequestService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<AdminPartRequestResponse>>>> GetPartRequests(
        [FromQuery] AdminPartRequestQueryRequest query,
        CancellationToken cancellationToken)
    {
        try
        {
            var partRequests = await _partRequestService.GetPartRequestsPageAsync(query, cancellationToken);
            return Ok(ApiResponse<PaginatedResponse<AdminPartRequestResponse>>.Ok(partRequests, "Part requests fetched successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PaginatedResponse<AdminPartRequestResponse>>.Fail(ex.Message));
        }
    }

    [HttpGet("{partRequestId:int}")]
    public async Task<ActionResult<ApiResponse<AdminPartRequestResponse>>> GetPartRequestById(
        int partRequestId,
        CancellationToken cancellationToken)
    {
        try
        {
            var partRequest = await _partRequestService.GetPartRequestByIdAsync(partRequestId, cancellationToken);
            return Ok(ApiResponse<AdminPartRequestResponse>.Ok(partRequest, "Part request fetched successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AdminPartRequestResponse>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AdminPartRequestResponse>.Fail(ex.Message));
        }
    }

    [HttpPatch("{partRequestId:int}/status")]
    public async Task<ActionResult<ApiResponse<AdminPartRequestResponse>>> UpdatePartRequestStatus(
        int partRequestId,
        UpdatePartRequestStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var partRequest = await _partRequestService.UpdatePartRequestStatusAsync(partRequestId, request, cancellationToken);
            return Ok(ApiResponse<AdminPartRequestResponse>.Ok(partRequest, "Part request status updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AdminPartRequestResponse>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AdminPartRequestResponse>.Fail(ex.Message));
        }
    }
}
