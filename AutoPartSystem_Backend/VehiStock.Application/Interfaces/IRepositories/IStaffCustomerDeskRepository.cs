using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IStaffCustomerDeskRepository
{
    Task<IReadOnlyCollection<CustomerProfile>> SearchCustomersAsync(
        string? fullname,
        string? customerPhone,
        string? vehicleNumber,
        int? customerId,
        string? emailId,
        CancellationToken cancellationToken = default);

    Task<CustomerProfile?> GetCustomerWithVehiclesAsync(int customerId, CancellationToken cancellationToken = default);
}
