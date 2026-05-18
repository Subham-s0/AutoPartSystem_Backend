using Microsoft.AspNetCore.Http;
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

    [HttpPost("{customerId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<CustomerVehicleResponse>>> CreateVehicleForCustomer(
        int customerId,
        [FromForm] CreateVehicleFormRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await _vehicleService.CreateVehicleForCustomerAsync(
                customerId,
                MapCreateRequest(request),
                cancellationToken);

            return Ok(ApiResponse<CustomerVehicleResponse>.Ok(vehicle, "Vehicle registered successfully for customer."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerVehicleResponse>.Fail(ex.Message));
        }
    }

    private static CreateVehicleRequest MapCreateRequest(CreateVehicleFormRequest request)
    {
        return new CreateVehicleRequest
        {
            VehicleNumber = request.VehicleNumber,
            Make = request.Make,
            Model = request.Model,
            ManufactureYear = request.ManufactureYear,
            EngineNo = request.EngineNo,
            ChassisNo = request.ChassisNo,
            VehiclePhoto = MapImageUploadFile(request.VehiclePhoto),
            MileageKm = request.MileageKm,
            Notes = request.Notes
        };
    }

    private static ImageUploadFile? MapImageUploadFile(IFormFile? file)
    {
        if (file is null) return null;
        return new ImageUploadFile(
            file.FileName,
            file.ContentType,
            file.Length,
            file.OpenReadStream,
            file.CopyToAsync);
    }
}
