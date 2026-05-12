using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface IVehicleService
{
    Task<IReadOnlyCollection<CustomerVehicleResponse>> GetVehiclesAsync(string userId, CancellationToken cancellationToken = default);
    Task<CustomerVehicleResponse> CreateVehicleAsync(string userId, CreateVehicleRequest request, CancellationToken cancellationToken = default);
    Task<CustomerVehicleResponse> UpdateVehicleAsync(string userId, int vehicleId, UpdateVehicleRequest request, CancellationToken cancellationToken = default);
    Task DeleteVehicleAsync(string userId, int vehicleId, CancellationToken cancellationToken = default);
}
