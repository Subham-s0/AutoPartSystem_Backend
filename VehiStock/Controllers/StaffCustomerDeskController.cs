using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/customer")]
public class StaffCustomerDeskController : ControllerBase
{
    private readonly IStaffCustomerDeskService _staffCustomerDeskService;

    public StaffCustomerDeskController(IStaffCustomerDeskService staffCustomerDeskService)
    {
        _staffCustomerDeskService = staffCustomerDeskService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerDeskDetailsResponse>>>> Search(
        [FromQuery] string? fullname,
        [FromQuery] string? customerPhone,
        [FromQuery] string? vehicleNumber,
        [FromQuery] int? customerId,
        [FromQuery] string? emailID,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = await _staffCustomerDeskService.SearchAsync(
                fullname,
                customerPhone,
                vehicleNumber,
                customerId,
                emailID,
                cancellationToken);

            return Ok(ApiResponse<IReadOnlyCollection<CustomerDeskDetailsResponse>>.Ok(
                results,
                "Customers fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IReadOnlyCollection<CustomerDeskDetailsResponse>>.Fail(ex.Message));
        }
    }

    [HttpGet("{customerId:int}")]
    public async Task<ActionResult<ApiResponse<CustomerDeskDetailsResponse>>> GetById(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var details = await _staffCustomerDeskService.GetDetailsAsync(customerId, cancellationToken);
            return Ok(ApiResponse<CustomerDeskDetailsResponse>.Ok(details, "Customer fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<CustomerDeskDetailsResponse>.Fail(ex.Message));
        }
    }

    [HttpGet("{customerId:int}/history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerDeskHistoryLineResponse>>>> GetHistory(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _staffCustomerDeskService.GetPurchaseHistoryAsync(customerId, cancellationToken);
            return Ok(ApiResponse<IReadOnlyCollection<CustomerDeskHistoryLineResponse>>.Ok(
                history,
                "Customer purchase history fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<IReadOnlyCollection<CustomerDeskHistoryLineResponse>>.Fail(ex.Message));
        }
    }
}
