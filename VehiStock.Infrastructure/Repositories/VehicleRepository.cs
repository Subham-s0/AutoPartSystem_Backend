using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly ApplicationDbContext _dbContext;

    public VehicleRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<CustomerProfile?> GetCustomerProfileByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .SingleOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Vehicle>> GetVehiclesForCustomerQueryAsync(
        int customerId,
        VehicleQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Vehicles
            .Where(x => x.CustomerId == customerId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim().ToLower();
            var yearSearch = searchText.All(char.IsDigit) ? searchText : null;

            query = query.Where(x =>
                x.VehicleNumber.ToLower().Contains(searchText) ||
                x.Make.ToLower().Contains(searchText) ||
                x.Model.ToLower().Contains(searchText) ||
                (x.EngineNo != null && x.EngineNo.ToLower().Contains(searchText)) ||
                (x.ChassisNo != null && x.ChassisNo.ToLower().Contains(searchText)) ||
                (x.Notes != null && x.Notes.ToLower().Contains(searchText)) ||
                (yearSearch != null && x.ManufactureYear.ToString().Contains(yearSearch)));
        }

        query = ApplySorting(query, request.Sorts);

        return await query.ToListAsync(cancellationToken);
    }

    public Task<Vehicle?> GetVehicleForCustomerAsync(int customerId, int vehicleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Vehicles
            .SingleOrDefaultAsync(x => x.CustomerId == customerId && x.VehicleId == vehicleId, cancellationToken);
    }

    public Task<bool> VehicleNumberExistsAsync(string vehicleNumber, int? excludedVehicleId = null, CancellationToken cancellationToken = default)
    {
        var normalizedVehicleNumber = vehicleNumber.Trim().ToUpperInvariant();

        return _dbContext.Vehicles.AnyAsync(
            x => x.VehicleNumber.ToUpper() == normalizedVehicleNumber &&
                 (!excludedVehicleId.HasValue || x.VehicleId != excludedVehicleId.Value),
            cancellationToken);
    }

    public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        _dbContext.Vehicles.Add(vehicle);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return vehicle;
    }

    public async Task<bool> HasVehicleReferencesAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Appointments.AnyAsync(x => x.VehicleId == vehicleId, cancellationToken) ||
               await _dbContext.SalesInvoices.AnyAsync(x => x.VehicleId == vehicleId, cancellationToken) ||
               await _dbContext.ServiceRecords.AnyAsync(x => x.VehicleId == vehicleId, cancellationToken) ||
               await _dbContext.PartRequests.AnyAsync(x => x.VehicleId == vehicleId, cancellationToken);
    }

    public void DeleteVehicle(Vehicle vehicle)
    {
        _dbContext.Vehicles.Remove(vehicle);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Vehicle> ApplySorting(IQueryable<Vehicle> query, List<SortRequest> sorts)
    {
        if (sorts.Count == 0)
        {
            return query.OrderByDescending(x => x.MileageKm).ThenBy(x => x.VehicleNumber);
        }

        IOrderedQueryable<Vehicle>? ordered = null;

        foreach (var sort in sorts)
        {
            var asc = sort.SortDirection == SortDirection.Asc;

            ordered = sort.SortBy.Trim().ToLowerInvariant() switch
            {
                "mileagekm" or "mileage" => ordered is null
                    ? asc ? query.OrderBy(x => x.MileageKm) : query.OrderByDescending(x => x.MileageKm)
                    : asc ? ordered.ThenBy(x => x.MileageKm) : ordered.ThenByDescending(x => x.MileageKm),
                "manufactureyear" or "year" => ordered is null
                    ? asc ? query.OrderBy(x => x.ManufactureYear) : query.OrderByDescending(x => x.ManufactureYear)
                    : asc ? ordered.ThenBy(x => x.ManufactureYear) : ordered.ThenByDescending(x => x.ManufactureYear),
                _ => ordered is null
                    ? asc ? query.OrderBy(x => x.MileageKm) : query.OrderByDescending(x => x.MileageKm)
                    : asc ? ordered.ThenBy(x => x.MileageKm) : ordered.ThenByDescending(x => x.MileageKm),
            };
        }

        return ordered!.ThenBy(x => x.VehicleNumber);
    }
}
