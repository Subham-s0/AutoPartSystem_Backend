using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

// Used for customer report endpoints
[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/staff/reports/customers")]
public class CustomerReportsController : ControllerBase
{
    private readonly IStaffReportService _staffReportService;

    public CustomerReportsController(IStaffReportService staffReportService)
    {
        _staffReportService = staffReportService;
    }

    [HttpGet("regulars")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<RegularCustomerReportResponse>>>> GetRegularCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int minimumInvoices = 2,
        CancellationToken cancellationToken = default)
    {
        var result = await _staffReportService.GetRegularCustomersAsync(pageNumber, pageSize, minimumInvoices, cancellationToken);
        return Ok(ApiResponse<PaginatedResponse<RegularCustomerReportResponse>>.Ok(result, "Regular customers fetched successfully."));
    }

    [HttpGet("high-spenders")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<HighSpenderReportResponse>>>> GetHighSpenders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _staffReportService.GetHighSpendersAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResponse<HighSpenderReportResponse>>.Ok(result, "High spenders fetched successfully."));
    }

    [HttpGet("pending-credits")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PendingCreditReportResponse>>>> GetPendingCredits(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _staffReportService.GetPendingCreditsAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResponse<PendingCreditReportResponse>>.Ok(result, "Pending credits fetched successfully."));
    }
}
