using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class CustomerProfileRepository : ICustomerProfileRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerProfileRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<CustomerProfile?> GetCustomerProfileByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    public async Task<PaginatedResponse<CustomerProfile>> GetCustomersForStaffAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CustomerProfiles
            .Include(x => x.User)
            .Include(x => x.Vehicles)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(x => 
                x.User.FullName.ToLower().Contains(searchLower) ||
                (x.User.PhoneNumber != null && x.User.PhoneNumber.ToLower().Contains(searchLower)) ||
                x.CustomerId.ToString() == searchLower ||
                x.Vehicles.Any(v => v.VehicleNumber.ToLower().Contains(searchLower)));
        }

        var totalRecords = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<CustomerProfile>
        {
            Items = items,
            TotalRecords = totalRecords,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
    }
}
