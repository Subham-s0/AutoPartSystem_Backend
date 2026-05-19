using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly ApplicationDbContext _context;

    public VendorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Vendor>> GetAllVendorsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .OrderBy(x => x.VendorName)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResponse<Vendor>> GetVendorsPaginatedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        IQueryable<Vendor> query = _context.Vendors;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim().ToLower();
            query = query.Where(x => 
                x.VendorName.ToLower().Contains(cleanSearch) ||
                x.VendorCode.ToLower().Contains(cleanSearch) ||
                (x.ContactPerson != null && x.ContactPerson.ToLower().Contains(cleanSearch)) ||
                (x.Email != null && x.Email.ToLower().Contains(cleanSearch)) ||
                (x.Phone != null && x.Phone.ToLower().Contains(cleanSearch))
            );
        }

        query = query.OrderBy(x => x.VendorName);

        var totalRecords = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<Vendor>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    public async Task<Vendor?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors.FindAsync(new object[] { vendorId }, cancellationToken);
    }

    public async Task<Vendor?> GetVendorByCodeAsync(string vendorCode, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .FirstOrDefaultAsync(x => x.VendorCode == vendorCode.ToUpper().Trim(), cancellationToken);
    }

    public async Task<Vendor> CreateVendorAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        await _context.Vendors.AddAsync(vendor, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return vendor;
    }

    public void DeleteVendor(Vendor vendor)
    {
        _context.Vendors.Remove(vendor);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
