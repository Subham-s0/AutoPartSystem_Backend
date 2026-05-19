using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Management;
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
    public async Task<(IReadOnlyCollection<CustomerDirectoryItemResponse> Items, int TotalRecords)> GetCustomersAsync(
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CustomerProfiles
            .AsNoTracking()
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
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(x =>
                x.User.FullName.ToLower().Contains(normalizedSearch) ||
                (x.User.Email != null && x.User.Email.ToLower().Contains(normalizedSearch)) ||
                (x.User.PhoneNumber != null && x.User.PhoneNumber.Contains(search.Trim())) ||
                x.Address.ToLower().Contains(normalizedSearch) ||
                x.CustomerId.ToString().Contains(search.Trim()) ||
                x.Vehicles.Any(v =>
                    v.VehicleNumber.ToLower().Contains(normalizedSearch) ||
                    v.Make.ToLower().Contains(normalizedSearch) ||
                    v.Model.ToLower().Contains(normalizedSearch)));
        }

        var totalRecords = await query.CountAsync(cancellationToken);
        var customers = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.User.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = customers.Select(MapCustomerItem).ToList();
        return (items, totalRecords);
    }

    public Task<CustomerProfile?> GetCustomerDetailByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Vehicles)
            .Include(x => x.SalesInvoices)
                .ThenInclude(x => x.Vehicle)
            .Include(x => x.ServiceInvoices)
            .Include(x => x.Payments)
                .ThenInclude(x => x.SalesInvoice)
            .SingleOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    private static CustomerDirectoryItemResponse MapCustomerItem(CustomerProfile customer)
    {
        return new CustomerDirectoryItemResponse
        {
            CustomerId = customer.CustomerId,
            UserId = customer.UserId,
            FullName = customer.User.FullName,
            Email = customer.User.Email ?? string.Empty,
            PhoneNumber = customer.User.PhoneNumber,
            ProfilePhotoUrl = customer.User.ProfilePhotoUrl,
            Address = customer.Address,
            RegistrationSource = customer.RegistrationSource,
            RegisteredAt = customer.CreatedAt,
            Vehicles = customer.Vehicles
                .OrderBy(v => v.VehicleNumber)
                .Select(v => new CustomerDirectoryVehicleResponse
                {
                    VehicleId = v.VehicleId,
                    VehicleNumber = v.VehicleNumber,
                    Make = v.Make,
                    Model = v.Model,
                    ManufactureYear = v.ManufactureYear,
                    MileageKm = v.MileageKm
                })
                .ToList()
        };
    }
}
