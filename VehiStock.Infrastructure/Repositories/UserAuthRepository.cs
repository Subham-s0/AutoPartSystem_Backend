using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class UserAuthRepository : IUserAuthRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserAuthRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfAppTransaction(transaction);
    }

    public async Task<CustomerProfile> CreateCustomerProfileAsync(
        string userId,
        string address,
        RegistrationSource registrationSource,
        CancellationToken cancellationToken = default)
    {
        var customerProfile = new CustomerProfile
        {
            UserId = userId,
            Address = address,
            RegistrationSource = registrationSource
        };

        _dbContext.CustomerProfiles.Add(customerProfile);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return customerProfile;
    }

    public async Task<StaffProfile> CreateStaffProfileAsync(
        string userId,
        string jobTitle,
        DateOnly hireDate,
        CancellationToken cancellationToken = default)
    {
        var staffProfile = new StaffProfile
        {
            UserId = userId,
            JobTitle = jobTitle,
            HireDate = hireDate
        };

        _dbContext.StaffProfiles.Add(staffProfile);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return staffProfile;
    }

    public async Task<RefreshToken?> GetRefreshTokenByHashAsync(
        string tokenHash,
        bool includeUser,
        CancellationToken cancellationToken = default)
    {
        IQueryable<RefreshToken> query = _dbContext.RefreshTokens;
        if (includeUser)
        {
            query = query.Include(x => x.User);
        }

        return await query.SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, DateTime revokedAt, CancellationToken cancellationToken = default)
    {
        refreshToken.RevokedAt = revokedAt;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeActiveRefreshTokensAsync(string userId, DateTime revokedAt, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(x => x.UserId == userId && !x.RevokedAt.HasValue && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (activeTokens.Count == 0)
        {
            return;
        }

        foreach (var token in activeTokens)
        {
            token.RevokedAt = revokedAt;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int?> GetCustomerIdByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (int?)x.CustomerId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<int?> GetStaffMemberIdByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.StaffProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (int?)x.StaffMemberId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private sealed class EfAppTransaction : IAppTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfAppTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return _transaction.CommitAsync(cancellationToken);
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            return _transaction.RollbackAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _transaction.DisposeAsync();
        }
    }
}
