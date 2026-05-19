using VehiStock.Application.Dtos.Customer;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IVehicleRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<CustomerProfile?> GetCustomerProfileByIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Vehicle>> GetVehiclesForCustomerQueryAsync(
        int customerId,
        VehicleQueryRequest request,
        CancellationToken cancellationToken = default);
    Task<Vehicle?> GetVehicleForCustomerAsync(int customerId, int vehicleId, CancellationToken cancellationToken = default);
    Task<bool> VehicleNumberExistsAsync(string vehicleNumber, int? excludedVehicleId = null, CancellationToken cancellationToken = default);
    Task<Vehicle> CreateVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task<bool> HasVehicleReferencesAsync(int vehicleId, CancellationToken cancellationToken = default);
    void DeleteVehicle(Vehicle vehicle);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
