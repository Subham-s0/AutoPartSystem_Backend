using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class VehicleService : IVehicleService
{
    private const string VehicleImageFolder = "vehicles";

    private readonly IVehicleRepository _vehicleRepository;
    private readonly IImageStorageService _imageStorageService;

    public VehicleService(
        IVehicleRepository vehicleRepository,
        IImageStorageService imageStorageService)
    {
        _vehicleRepository = vehicleRepository;
        _imageStorageService = imageStorageService;
    }

    public async Task<IReadOnlyCollection<CustomerVehicleResponse>> GetVehiclesAsync(
        string userId,
        VehicleQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var vehicles = await _vehicleRepository.GetVehiclesForCustomerQueryAsync(
            customer.CustomerId,
            NormalizeVehicleQuery(query),
            cancellationToken);
        return vehicles.Select(MapVehicle).ToList();
    }

    public async Task<IReadOnlyCollection<CustomerVehicleResponse>> GetVehiclesForCustomerAsync(
        int customerId,
        VehicleQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var customer = await _vehicleRepository.GetCustomerProfileByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer was not found.");
        }

        var vehicles = await _vehicleRepository.GetVehiclesForCustomerQueryAsync(
            customerId,
            NormalizeVehicleQuery(query),
            cancellationToken);
        return vehicles.Select(MapVehicle).ToList();
    }

    public async Task<CustomerVehicleResponse> CreateVehicleAsync(string userId, CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var vehicleNumber = request.VehicleNumber.Trim();

        if (await _vehicleRepository.VehicleNumberExistsAsync(vehicleNumber, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("A vehicle with this number is already registered.");
        }

        string? vehiclePhotoUrl = null;

        try
        {
            if (request.VehiclePhoto is not null)
            {
                vehiclePhotoUrl = await _imageStorageService.SaveImageAsync(
                    request.VehiclePhoto,
                    VehicleImageFolder,
                    cancellationToken);
            }

            var vehicle = new Vehicle
            {
                CustomerId = customer.CustomerId,
                VehicleNumber = vehicleNumber,
                Make = request.Make.Trim(),
                Model = request.Model.Trim(),
                ManufactureYear = request.ManufactureYear,
                EngineNo = NormalizeOptionalText(request.EngineNo),
                ChassisNo = NormalizeOptionalText(request.ChassisNo),
                VehiclePhotoUrl = vehiclePhotoUrl,
                MileageKm = request.MileageKm,
                Notes = NormalizeOptionalText(request.Notes)
            };

            var createdVehicle = await _vehicleRepository.CreateVehicleAsync(vehicle, cancellationToken);
            return MapVehicle(createdVehicle);
        }
        catch
        {
            _imageStorageService.DeleteImage(vehiclePhotoUrl);
            throw;
        }
    }

    public async Task<CustomerVehicleResponse> UpdateVehicleAsync(string userId, int vehicleId, UpdateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var vehicle = await _vehicleRepository.GetVehicleForCustomerAsync(customer.CustomerId, vehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new InvalidOperationException("Vehicle not found for this customer.");
        }

        var vehicleNumber = request.VehicleNumber.Trim();
        if (await _vehicleRepository.VehicleNumberExistsAsync(vehicleNumber, vehicle.VehicleId, cancellationToken))
        {
            throw new InvalidOperationException("A vehicle with this number is already registered.");
        }

        var oldVehiclePhotoUrl = vehicle.VehiclePhotoUrl;
        var newVehiclePhotoUrl = oldVehiclePhotoUrl;

        try
        {
            if (request.VehiclePhoto is not null)
            {
                newVehiclePhotoUrl = await _imageStorageService.SaveImageAsync(
                    request.VehiclePhoto,
                    VehicleImageFolder,
                    cancellationToken);
            }
            else if (request.RemoveVehiclePhoto)
            {
                newVehiclePhotoUrl = null;
            }

            vehicle.VehicleNumber = vehicleNumber;
            vehicle.Make = request.Make.Trim();
            vehicle.Model = request.Model.Trim();
            vehicle.ManufactureYear = request.ManufactureYear;
            vehicle.EngineNo = NormalizeOptionalText(request.EngineNo);
            vehicle.ChassisNo = NormalizeOptionalText(request.ChassisNo);
            vehicle.VehiclePhotoUrl = newVehiclePhotoUrl;
            vehicle.MileageKm = request.MileageKm;
            vehicle.Notes = NormalizeOptionalText(request.Notes);

            await _vehicleRepository.SaveChangesAsync(cancellationToken);

            if (request.VehiclePhoto is not null || request.RemoveVehiclePhoto)
            {
                _imageStorageService.DeleteImage(oldVehiclePhotoUrl);
            }

            return MapVehicle(vehicle);
        }
        catch
        {
            if (!string.Equals(newVehiclePhotoUrl, oldVehiclePhotoUrl, StringComparison.Ordinal))
            {
                _imageStorageService.DeleteImage(newVehiclePhotoUrl);
            }

            throw;
        }
    }

    public async Task DeleteVehicleAsync(string userId, int vehicleId, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var vehicle = await _vehicleRepository.GetVehicleForCustomerAsync(customer.CustomerId, vehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new InvalidOperationException("Vehicle not found for this customer.");
        }

        if (await _vehicleRepository.HasVehicleReferencesAsync(vehicle.VehicleId, cancellationToken))
        {
            throw new InvalidOperationException("Vehicles with appointments, invoices, service records, or part requests cannot be deleted.");
        }

        var vehiclePhotoUrl = vehicle.VehiclePhotoUrl;

        _vehicleRepository.DeleteVehicle(vehicle);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);
        _imageStorageService.DeleteImage(vehiclePhotoUrl);
    }

    private async Task<CustomerProfile> GetCustomerProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var customer = await _vehicleRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer profile was not found for this account.");
        }

        return customer;
    }

    private static CustomerVehicleResponse MapVehicle(Vehicle vehicle)
    {
        return new CustomerVehicleResponse
        {
            VehicleId = vehicle.VehicleId,
            VehicleNumber = vehicle.VehicleNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            ManufactureYear = vehicle.ManufactureYear,
            EngineNo = vehicle.EngineNo,
            ChassisNo = vehicle.ChassisNo,
            VehiclePhotoUrl = vehicle.VehiclePhotoUrl,
            MileageKm = vehicle.MileageKm,
            Notes = vehicle.Notes
        };
    }

    private static VehicleQueryRequest NormalizeVehicleQuery(VehicleQueryRequest query)
    {
        var normalized = new VehicleQueryRequest
        {
            SearchText = string.IsNullOrWhiteSpace(query.SearchText) ? null : query.SearchText.Trim(),
            Sorts = query.Sorts
                .Where(s => !string.IsNullOrWhiteSpace(s.SortBy))
                .Select(s => new SortRequest
                {
                    SortBy = s.SortBy.Trim(),
                    SortDirection = s.SortDirection
                })
                .ToList()
        };

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
