using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
[Route("api/staff/customers")]
public class StaffCustomersController : ControllerBase
{
    private readonly ICustomerProfileService _customerProfileService;

    public StaffCustomersController(ICustomerProfileService customerProfileService)
    {
        _customerProfileService = customerProfileService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StaffCustomerResponse>>>> SearchCustomers(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var customers = await _customerProfileService.GetCustomersForStaffAsync(search, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResponse<StaffCustomerResponse>>.Ok(customers, "Customers fetched successfully."));
    }

    [HttpGet("{customerId:int}/history")]
    public async Task<ActionResult<ApiResponse<StaffCustomerHistoryResponse>>> GetCustomerHistory(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _customerProfileService.GetCustomerHistoryAsync(customerId, cancellationToken);
            return Ok(ApiResponse<StaffCustomerHistoryResponse>.Ok(history, "Customer history fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<StaffCustomerHistoryResponse>.Fail(ex.Message));
        }
    }
}
