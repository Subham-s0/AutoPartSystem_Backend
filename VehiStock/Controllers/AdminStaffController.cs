using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Common;
using VehiStock.Application.DTOs.Staff;
using VehiStock.Application.Interfaces.IServices;

namespace VehiStock.Controllers;

// Used for auth management endpoints
[ApiController]
[Route("api/admin/staff")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminStaffController(IStaffAdministrationService staffAdministrationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<StaffSummaryDto>> RegisterStaff(RegisterStaffRequest request, CancellationToken cancellationToken)
    {
        var staff = await staffAdministrationService.RegisterStaffAsync(request, cancellationToken);
        return Ok(staff);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffSummaryDto>>> GetStaff(CancellationToken cancellationToken)
    {
        var staff = await staffAdministrationService.GetStaffAsync(cancellationToken);
        return Ok(staff);
    }

    [HttpPut("{userId}/role")]
    public async Task<ActionResult<StaffSummaryDto>> UpdateRole(string userId, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var staff = await staffAdministrationService.UpdateRoleAsync(userId, request, cancellationToken);
        return Ok(staff);
    }
}
