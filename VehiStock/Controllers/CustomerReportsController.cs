using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Common;
using VehiStock.Application.DTOs.Reports;
using VehiStock.Application.Interfaces.IServices;

namespace VehiStock.Controllers;

// Used for customer report endpoints
[ApiController]
[Route("api/staff/reports/customers")]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
public class CustomerReportsController(ICustomerReportService customerReportService) : ControllerBase
{
    [HttpGet("regulars")]
    public async Task<ActionResult<IReadOnlyList<RegularCustomerReportItemDto>>> GetRegularCustomers(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int topCount = 10,
        [FromQuery] int minimumInvoices = 2,
        CancellationToken cancellationToken = default)
    {
        var result = await customerReportService.GetRegularCustomersAsync(new CustomerReportFilterDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            TopCount = topCount,
            MinimumInvoices = minimumInvoices
        }, cancellationToken);

        return Ok(result);
    }

    [HttpGet("high-spenders")]
    public async Task<ActionResult<IReadOnlyList<HighSpenderReportItemDto>>> GetHighSpenders(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await customerReportService.GetHighSpendersAsync(new CustomerReportFilterDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            TopCount = topCount
        }, cancellationToken);

        return Ok(result);
    }

    [HttpGet("pending-credits")]
    public async Task<ActionResult<IReadOnlyList<PendingCreditReportItemDto>>> GetPendingCredits(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        var result = await customerReportService.GetPendingCreditsAsync(new CustomerReportFilterDto
        {
            FromDate = fromDate,
            ToDate = toDate
        }, cancellationToken);

        return Ok(result);
    }
}
