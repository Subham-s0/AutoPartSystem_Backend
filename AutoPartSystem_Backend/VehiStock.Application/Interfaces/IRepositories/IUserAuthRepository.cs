using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IUserAuthRepository
{
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<CustomerProfile> CreateCustomerProfileAsync(
        string userId,
        string address,
        RegistrationSource registrationSource,
        CancellationToken cancellationToken = default);
    Task<StaffProfile> CreateStaffProfileAsync(
        string userId,
        string jobTitle,
        DateOnly hireDate,
        CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetRefreshTokenByHashAsync(
        string tokenHash,
        bool includeUser,
        CancellationToken cancellationToken = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, DateTime revokedAt, CancellationToken cancellationToken = default);
    Task RevokeActiveRefreshTokensAsync(string userId, DateTime revokedAt, CancellationToken cancellationToken = default);
    Task<int?> GetCustomerIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<int?> GetStaffMemberIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
