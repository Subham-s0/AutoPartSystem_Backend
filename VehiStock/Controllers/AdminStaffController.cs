using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

// Used for auth management endpoints
[ApiController]
[Authorize(Roles = RoleNames.Admin)]
[Route("api/admin/staff")]
public class AdminStaffController : ControllerBase
{
    private readonly IStaffManagementService _staffManagementService;

    public AdminStaffController(IStaffManagementService staffManagementService)
    {
        _staffManagementService = staffManagementService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StaffSummaryResponse>>>> GetStaff(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var staff = await _staffManagementService.GetStaffAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResponse<StaffSummaryResponse>>.Ok(staff, "Staff fetched successfully."));
    }

    [HttpPut("{userId}/role")]
    public async Task<ActionResult<ApiResponse<StaffSummaryResponse>>> UpdateRole(
        string userId,
        UpdateStaffRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var staff = await _staffManagementService.UpdateRoleAsync(userId, request, cancellationToken);
            return Ok(ApiResponse<StaffSummaryResponse>.Ok(staff, "Staff role updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<StaffSummaryResponse>.Fail(ex.Message));
        }
    }
}
