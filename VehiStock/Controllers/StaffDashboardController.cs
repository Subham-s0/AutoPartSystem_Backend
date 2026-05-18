using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/staff/dashboard")]
public class StaffDashboardController : ControllerBase
{
    private readonly IStaffDashboardService _dashboardService;

    public StaffDashboardController(IStaffDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<StaffDashboardResponse>>> GetDashboardSummary(
        CancellationToken cancellationToken = default)
    {
        var summary = await _dashboardService.GetDashboardSummaryAsync(cancellationToken);
        return Ok(ApiResponse<StaffDashboardResponse>.Ok(summary, "Staff dashboard summary fetched successfully."));
    }
}
