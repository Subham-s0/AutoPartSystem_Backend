using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;

    public VendorService(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<IReadOnlyCollection<VendorResponse>> GetAllVendorsAsync(CancellationToken cancellationToken = default)
    {
        var vendors = await _vendorRepository.GetAllVendorsAsync(cancellationToken);
        return vendors.Select(MapToResponse).ToList();
    }

    public async Task<PaginatedResponse<VendorResponse>> GetVendorsPaginatedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _vendorRepository.GetVendorsPaginatedAsync(search, pageNumber, pageSize, cancellationToken);
        var mappedItems = result.Items.Select(MapToResponse).ToList();

        return new PaginatedResponse<VendorResponse>
        {
            Items = mappedItems,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    public async Task<VendorResponse> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);
        if (vendor == null)
        {
            throw new InvalidOperationException("Vendor not found.");
        }

        return MapToResponse(vendor);
    }

    public async Task<VendorResponse> CreateVendorAsync(CreateVendorRequest request, CancellationToken cancellationToken = default)
    {
        var existingVendor = await _vendorRepository.GetVendorByCodeAsync(request.VendorCode, cancellationToken);
        if (existingVendor != null)
        {
            throw new InvalidOperationException($"Vendor with code '{request.VendorCode}' already exists.");
        }

        var vendor = new Vendor
        {
            VendorCode = request.VendorCode.ToUpper().Trim(),
            VendorName = request.VendorName.Trim(),
            ContactPerson = request.ContactPerson.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim(),
            Address = request.Address.Trim(),
            IsActive = true
        };

        var createdVendor = await _vendorRepository.CreateVendorAsync(vendor, cancellationToken);
        return MapToResponse(createdVendor);
    }

    public async Task<VendorResponse> UpdateVendorAsync(int vendorId, UpdateVendorRequest request, CancellationToken cancellationToken = default)
    {
        var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);
        if (vendor == null)
        {
            throw new InvalidOperationException("Vendor not found.");
        }

        vendor.VendorName = request.VendorName.Trim();
        vendor.ContactPerson = request.ContactPerson.Trim();
        vendor.Phone = request.Phone.Trim();
        vendor.Email = request.Email.Trim();
        vendor.Address = request.Address.Trim();
        vendor.IsActive = request.IsActive;

        await _vendorRepository.SaveChangesAsync(cancellationToken);
        return MapToResponse(vendor);
    }

    public async Task DeleteVendorAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);
        if (vendor == null)
        {
            throw new InvalidOperationException("Vendor not found.");
        }

        _vendorRepository.DeleteVendor(vendor);
        await _vendorRepository.SaveChangesAsync(cancellationToken);
    }

    private static VendorResponse MapToResponse(Vendor vendor)
    {
        return new VendorResponse
        {
            VendorId = vendor.VendorId,
            VendorCode = vendor.VendorCode,
            VendorName = vendor.VendorName,
            ContactPerson = vendor.ContactPerson,
            Phone = vendor.Phone,
            Email = vendor.Email,
            Address = vendor.Address,
            IsActive = vendor.IsActive
        };
    }
}
