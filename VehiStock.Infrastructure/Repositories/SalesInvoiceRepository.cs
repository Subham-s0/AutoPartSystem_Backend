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
        return _dbContext.CustomerProfiles.SingleOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
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
}
