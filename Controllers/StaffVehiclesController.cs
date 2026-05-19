using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;
using VehiStock.Entities;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.Staff},{RoleNames.Admin}")]
[Route("api/staff/vehicles")]
public class StaffVehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IImageStorageService _imageStorageService;

    public StaffVehiclesController(
        IVehicleService vehicleService,
        IVehicleRepository vehicleRepository,
        IImageStorageService imageStorageService)
    {
        _vehicleService = vehicleService;
        _vehicleRepository = vehicleRepository;
        _imageStorageService = imageStorageService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerVehicleResponse>>>> GetVehicles(
        [FromQuery] int customerId,
        [FromQuery] string? searchText = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new VehicleQueryRequest { SearchText = searchText };
            var vehicles = await _vehicleService.GetVehiclesForCustomerAsync(customerId, query, cancellationToken);
            return Ok(ApiResponse<IReadOnlyCollection<CustomerVehicleResponse>>.Ok(vehicles, "Vehicles fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IReadOnlyCollection<CustomerVehicleResponse>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IReadOnlyCollection<CustomerVehicleResponse>>.Fail("An error occurred: " + ex.Message));
        }
    }

    [HttpPost("{customerId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<CustomerVehicleResponse>>> CreateVehicleForCustomer(
        int customerId,
        [FromForm] CreateVehicleFormRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _vehicleRepository.GetCustomerProfileByIdAsync(customerId, cancellationToken);
            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerVehicleResponse>.Fail("Customer profile was not found."));
            }

            var vehicleNumber = request.VehicleNumber.Trim();
            if (await _vehicleRepository.VehicleNumberExistsAsync(vehicleNumber, cancellationToken: cancellationToken))
            {
                return BadRequest(ApiResponse<CustomerVehicleResponse>.Fail("A vehicle with this number is already registered."));
            }

            string? vehiclePhotoUrl = null;
            if (request.VehiclePhoto is not null)
            {
                var mapPhoto = MapImageUploadFile(request.VehiclePhoto);
                if (mapPhoto != null)
                {
                    vehiclePhotoUrl = await _imageStorageService.SaveImageAsync(
                        mapPhoto,
                        "vehicles",
                        cancellationToken);
                }
            }

            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                VehicleNumber = vehicleNumber,
                Make = request.Make.Trim(),
                Model = request.Model.Trim(),
                ManufactureYear = request.ManufactureYear,
                EngineNo = string.IsNullOrWhiteSpace(request.EngineNo) ? null : request.EngineNo.Trim(),
                ChassisNo = string.IsNullOrWhiteSpace(request.ChassisNo) ? null : request.ChassisNo.Trim(),
                VehiclePhotoUrl = vehiclePhotoUrl,
                MileageKm = request.MileageKm,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
            };

            var createdVehicle = await _vehicleRepository.CreateVehicleAsync(vehicle, cancellationToken);
            
            var response = new CustomerVehicleResponse
            {
                VehicleId = createdVehicle.VehicleId,
                VehicleNumber = createdVehicle.VehicleNumber,
                Make = createdVehicle.Make,
                Model = createdVehicle.Model,
                ManufactureYear = createdVehicle.ManufactureYear,
                EngineNo = createdVehicle.EngineNo,
                ChassisNo = createdVehicle.ChassisNo,
                VehiclePhotoUrl = createdVehicle.VehiclePhotoUrl,
                MileageKm = createdVehicle.MileageKm,
                Notes = createdVehicle.Notes
            };

            return Ok(ApiResponse<CustomerVehicleResponse>.Ok(response, "Vehicle registered successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerVehicleResponse>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CustomerVehicleResponse>.Fail("An error occurred: " + ex.Message));
        }
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
