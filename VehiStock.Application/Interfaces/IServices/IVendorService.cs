using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Interfaces.IServices;

public interface IVendorService
{
    Task<IReadOnlyCollection<VendorResponse>> GetAllVendorsAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResponse<VendorResponse>> GetVendorsPaginatedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<VendorResponse> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default);
    Task<VendorResponse> CreateVendorAsync(CreateVendorRequest request, CancellationToken cancellationToken = default);
    Task<VendorResponse> UpdateVendorAsync(int vendorId, UpdateVendorRequest request, CancellationToken cancellationToken = default);
    Task DeleteVendorAsync(int vendorId, CancellationToken cancellationToken = default);
}
