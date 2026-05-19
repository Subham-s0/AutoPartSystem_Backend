using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Management;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
[Route("api/admin/customers")]
public class AdminCustomersController : ControllerBase
{
    private readonly ICustomerProfileService _customerProfileService;

    public AdminCustomersController(ICustomerProfileService customerProfileService)
    {
        _customerProfileService = customerProfileService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CustomerDirectoryItemResponse>>>> GetCustomers(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var customers = await _customerProfileService.GetCustomersAsync(search, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResponse<CustomerDirectoryItemResponse>>.Ok(customers, "Customers fetched successfully."));
    }

    [HttpGet("{customerId:int}")]
    public async Task<ActionResult<ApiResponse<CustomerDirectoryDetailResponse>>> GetCustomerDetail(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _customerProfileService.GetCustomerDetailAsync(customerId, cancellationToken);
            return Ok(ApiResponse<CustomerDirectoryDetailResponse>.Ok(customer, "Customer profile fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<CustomerDirectoryDetailResponse>.Fail(ex.Message));
        }
    }
}
