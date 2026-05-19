using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class StaffCustomerDeskRepository : IStaffCustomerDeskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StaffCustomerDeskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CustomerProfile>> SearchCustomersAsync(
        string? fullname,
        string? customerPhone,
        string? vehicleNumber,
        int? customerId,
        string? emailId,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildCustomerQueryAsync(cancellationToken);

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(fullname))
        {
            var term = fullname.Trim().ToLower();
            query = query.Where(x => x.User.FullName.ToLower().Contains(term));
        }
        else if (!string.IsNullOrWhiteSpace(customerPhone))
        {
            var term = customerPhone.Trim().ToLower();
            query = query.Where(x =>
                x.User.PhoneNumber != null &&
                x.User.PhoneNumber.ToLower().Contains(term));
        }
        else if (!string.IsNullOrWhiteSpace(vehicleNumber))
        {
            var term = vehicleNumber.Trim().ToLower();
            query = query.Where(x =>
                x.Vehicles.Any(v => v.VehicleNumber.ToLower().Contains(term)));
        }
        else if (!string.IsNullOrWhiteSpace(emailId))
        {
            var term = emailId.Trim().ToLower();
            query = query.Where(x =>
                x.User.Email != null &&
                x.User.Email.ToLower().Contains(term));
        }
        else
        {
            return await query
                .OrderBy(x => x.User.FullName)
                .Take(200)
                .ToListAsync(cancellationToken);
        }

        return await query
            .OrderBy(x => x.User.FullName)
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerProfile?> GetCustomerWithVehiclesAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildCustomerQueryAsync(cancellationToken);
        return await query.SingleOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    private async Task<IQueryable<CustomerProfile>> BuildCustomerQueryAsync(CancellationToken cancellationToken)
    {
        var customerRoleId = await _dbContext.Roles
            .Where(x => x.Name == RoleNames.Customer)
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);

        return _dbContext.CustomerProfiles
            .Include(x => x.User)
            .Include(x => x.Vehicles)
            .Where(x => _dbContext.UserRoles.Any(ur => ur.UserId == x.UserId && ur.RoleId == customerRoleId))
            .AsQueryable();
    }
}
