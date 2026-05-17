using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface ICustomerProfileRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
