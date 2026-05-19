using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

// Implementation for sales invoice data access
public class SalesInvoiceRepository : ISalesInvoiceRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SalesInvoiceRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StaffProfile?> GetStaffProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.StaffProfiles
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<bool> InvoiceExistsAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        return _dbContext.SalesInvoices.AnyAsync(x => x.InvoiceNo == invoiceNo, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerProfile>> GetCustomersWithVehiclesAsync(CancellationToken cancellationToken = default)
    {
        var customerRoleId = await _dbContext.Roles
            .Where(x => x.Name == RoleNames.Customer)
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);

        return await _dbContext.CustomerProfiles
            .Include(x => x.User)
            .Include(x => x.Vehicles)
            .Where(x => _dbContext.UserRoles.Any(ur => ur.UserId == x.UserId && ur.RoleId == customerRoleId))
            .OrderBy(x => x.User.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Part>> GetAvailablePartsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Parts
            .Where(x => x.IsActive && x.StockQty > 0)
            .OrderBy(x => x.PartName)
            .ThenBy(x => x.Brand)
            .ToListAsync(cancellationToken);
    }

    public Task<CustomerProfile?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    public Task<Vehicle?> GetVehicleForCustomerAsync(int customerId, int vehicleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Vehicles.SingleOrDefaultAsync(x => x.CustomerId == customerId && x.VehicleId == vehicleId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Part>> GetPartsByIdsAsync(IReadOnlyCollection<int> partIds, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Parts
            .Where(x => partIds.Contains(x.PartId))
            .ToListAsync(cancellationToken);
    }

    public async Task<SalesInvoice> CreateSalesInvoiceAsync(SalesInvoice salesInvoice, Payment? payment, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.SalesInvoices.Add(salesInvoice);
        if (payment is not null)
        {
            _dbContext.Payments.Add(payment);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return salesInvoice;
    }

    public async Task<(IReadOnlyCollection<SalesInvoice> Items, int TotalRecords)> GetPaginatedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = BuildInvoiceQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(x =>
                x.InvoiceNo.ToLower().Contains(normalizedSearch) ||
                x.Customer.User.FullName.ToLower().Contains(normalizedSearch) ||
                x.Vehicle.VehicleNumber.ToLower().Contains(normalizedSearch) ||
                x.StaffMember.User.FullName.ToLower().Contains(normalizedSearch));
        }

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.SalesInvoiceId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalRecords);
    }

    public Task<SalesInvoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return BuildInvoiceQuery().SingleOrDefaultAsync(x => x.SalesInvoiceId == id, cancellationToken);
    }

    public async Task DeleteAsync(SalesInvoice salesInvoice, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var item in salesInvoice.Items)
        {
            item.Part.IncreaseStock(item.Quantity);
        }

        var payments = await _dbContext.Payments
            .Where(x => x.SalesInvoiceId == salesInvoice.SalesInvoiceId)
            .ToListAsync(cancellationToken);

        if (payments.Count > 0)
        {
            _dbContext.Payments.RemoveRange(payments);
        }

        _dbContext.SalesInvoices.Remove(salesInvoice);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<SalesInvoice> BuildInvoiceQuery()
    {
        return _dbContext.SalesInvoices
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .Include(x => x.Vehicle)
            .Include(x => x.StaffMember)
                .ThenInclude(x => x.User)
            .Include(x => x.Items)
                .ThenInclude(x => x.Part);
    }
}
