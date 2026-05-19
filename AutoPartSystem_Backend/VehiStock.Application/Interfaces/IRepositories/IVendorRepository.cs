using VehiStock.Application.Dtos.Common;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IVendorRepository
{
    Task<IReadOnlyCollection<Vendor>> GetAllVendorsAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Vendor>> GetVendorsPaginatedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Vendor?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default);
    Task<Vendor?> GetVendorByCodeAsync(string vendorCode, CancellationToken cancellationToken = default);
    Task<Vendor> CreateVendorAsync(Vendor vendor, CancellationToken cancellationToken = default);
    void DeleteVendor(Vendor vendor);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
