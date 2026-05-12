using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;
using VehiStock.Dtos.Customer;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Customer)]
[Route("api/customer/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerVehicleResponse>>>> GetVehicles(
        CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleService.GetVehiclesAsync(GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<CustomerVehicleResponse>>.Ok(vehicles, "Vehicles fetched successfully."));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<CustomerVehicleResponse>>> CreateVehicle(
        [FromForm] CreateVehicleFormRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await _vehicleService.CreateVehicleAsync(
                GetCurrentUserId(),
                MapCreateRequest(request),
                cancellationToken);

            return Ok(ApiResponse<CustomerVehicleResponse>.Ok(vehicle, "Vehicle registered successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerVehicleResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("{vehicleId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<CustomerVehicleResponse>>> UpdateVehicle(
        int vehicleId,
        [FromForm] UpdateVehicleFormRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await _vehicleService.UpdateVehicleAsync(
                GetCurrentUserId(),
                vehicleId,
                MapUpdateRequest(request),
                cancellationToken);

            return Ok(ApiResponse<CustomerVehicleResponse>.Ok(vehicle, "Vehicle updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerVehicleResponse>.Fail(ex.Message));
        }
    }

    [HttpDelete("{vehicleId:int}")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteVehicle(
        int vehicleId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _vehicleService.DeleteVehicleAsync(GetCurrentUserId(), vehicleId, cancellationToken);
            return Ok(ApiResponse<object?>.Ok(null, "Vehicle deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Authenticated user ID is missing.");
        }

        return userId;
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

    private static UpdateVehicleRequest MapUpdateRequest(UpdateVehicleFormRequest request)
    {
        return new UpdateVehicleRequest
        {
            VehicleNumber = request.VehicleNumber,
            Make = request.Make,
            Model = request.Model,
            ManufactureYear = request.ManufactureYear,
            EngineNo = request.EngineNo,
            ChassisNo = request.ChassisNo,
            VehiclePhoto = MapImageUploadFile(request.VehiclePhoto),
            RemoveVehiclePhoto = request.RemoveVehiclePhoto,
            MileageKm = request.MileageKm,
            Notes = request.Notes
        };
    }

    private static ImageUploadFile? MapImageUploadFile(IFormFile? file)
    {
        if (file is null)
        {
            return null;
        }

        return new ImageUploadFile(
            file.FileName,
            file.ContentType,
            file.Length,
            file.OpenReadStream,
            file.CopyToAsync);
    }
}
