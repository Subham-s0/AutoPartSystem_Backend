using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyCollection<Vehicle>> GetVehiclesByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vehicles
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.VehicleNumber)
            .ToListAsync(cancellationToken);
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
}
