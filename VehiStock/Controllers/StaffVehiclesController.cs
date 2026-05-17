using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/staff/vehicles")]
public class StaffVehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public StaffVehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerVehicleResponse>>>> GetVehiclesForCustomer(
        [FromQuery] int customerId,
        [FromQuery] VehicleQueryRequest? query,
        CancellationToken cancellationToken)
    {
        if (customerId <= 0)
        {
            return BadRequest(ApiResponse<IReadOnlyCollection<CustomerVehicleResponse>>.Fail("A valid customerId is required."));
        }

        try
        {
            var vehicles = await _vehicleService.GetVehiclesForCustomerAsync(
                customerId,
                query ?? new VehicleQueryRequest(),
                cancellationToken);

            return Ok(ApiResponse<IReadOnlyCollection<CustomerVehicleResponse>>.Ok(vehicles, "Vehicles fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IReadOnlyCollection<CustomerVehicleResponse>>.Fail(ex.Message));
        }
    }
}
