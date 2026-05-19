using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
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

    public async Task<IReadOnlyCollection<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SalesInvoices
            .Include(x => x.Customer)
                .ThenInclude(c => c.User)
            .Include(x => x.Vehicle)
            .Include(x => x.StaffMember)
                .ThenInclude(s => s.User)
            .Include(x => x.Items)
                .ThenInclude(i => i.Part)
            .OrderByDescending(x => x.SalesInvoiceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResponse<SalesInvoice>> GetSalesInvoicesPaginatedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        IQueryable<SalesInvoice> query = _dbContext.SalesInvoices
            .Include(x => x.Customer)
                .ThenInclude(c => c.User)
            .Include(x => x.Vehicle)
            .Include(x => x.StaffMember)
                .ThenInclude(s => s.User)
            .Include(x => x.Items)
                .ThenInclude(i => i.Part);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim().ToLower();
            query = query.Where(x => 
                x.InvoiceNo.ToLower().Contains(cleanSearch) ||
                (x.Customer != null && x.Customer.User != null && x.Customer.User.FullName.ToLower().Contains(cleanSearch)) ||
                (x.Vehicle != null && x.Vehicle.VehicleNumber.ToLower().Contains(cleanSearch))
            );
        }

        query = query.OrderByDescending(x => x.SalesInvoiceId);

        var totalRecords = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<SalesInvoice>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    public Task<SalesInvoice?> GetSalesInvoiceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.SalesInvoices
            .Include(x => x.Customer)
                .ThenInclude(c => c.User)
            .Include(x => x.Vehicle)
            .Include(x => x.StaffMember)
                .ThenInclude(s => s.User)
            .Include(x => x.Items)
                .ThenInclude(i => i.Part)
            .SingleOrDefaultAsync(x => x.SalesInvoiceId == id, cancellationToken);
    }

    public async Task DeleteSalesInvoiceAsync(SalesInvoice salesInvoice, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Clean up associated payments manually to avoid cascade path conflicts with DB foreign keys
        var payments = await _dbContext.Payments.Where(x => x.SalesInvoiceId == salesInvoice.SalesInvoiceId).ToListAsync(cancellationToken);
        _dbContext.Payments.RemoveRange(payments);

        var items = await _dbContext.SalesInvoiceItems.Where(x => x.SalesInvoiceId == salesInvoice.SalesInvoiceId).ToListAsync(cancellationToken);
        _dbContext.SalesInvoiceItems.RemoveRange(items);

        // Restore stock levels when deleting an invoice!
        foreach (var item in items)
        {
            var part = await _dbContext.Parts.FindAsync(new object[] { item.PartId }, cancellationToken);
            if (part is not null)
            {
                part.IncreaseStock(item.Quantity);
            }
        }

        _dbContext.SalesInvoices.Remove(salesInvoice);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
