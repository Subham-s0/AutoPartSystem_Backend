using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface ICustomerProfileService
{
    Task<CustomerProfileResponse> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
}
