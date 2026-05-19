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
[Route("api/customer/dashboard")]
public class CustomerDashboardController : ControllerBase
{
    private readonly ICustomerDashboardService _dashboardService;

    public CustomerDashboardController(ICustomerDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CustomerDashboardResponse>>> GetDashboardSummary(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _dashboardService.GetDashboardSummaryAsync(GetCurrentUserId(), cancellationToken);
            return Ok(ApiResponse<CustomerDashboardResponse>.Ok(summary, "Customer dashboard summary fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerDashboardResponse>.Fail(ex.Message));
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Authenticated user ID is missing.");
        return userId;
    }
}
