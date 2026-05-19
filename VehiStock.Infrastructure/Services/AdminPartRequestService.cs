using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class AdminPartRequestService : IAdminPartRequestService
{
    private readonly IAdminPartRequestRepository _repository;

    public AdminPartRequestService(IAdminPartRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResponse<AdminPartRequestResponse>> GetPartRequestsPageAsync(
        AdminPartRequestQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var page = await _repository.GetPartRequestsPageAsync(query, cancellationToken);

        return new PaginatedResponse<AdminPartRequestResponse>
        {
            Items = page.Items.Select(MapToAdminResponse).ToList(),
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalRecords = page.TotalRecords,
            TotalPages = page.TotalPages
        };
    }

    public async Task<AdminPartRequestResponse> GetPartRequestByIdAsync(
        int partRequestId,
        CancellationToken cancellationToken = default)
    {
        var partRequest = await _repository.GetPartRequestByIdAsync(partRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Part request with ID {partRequestId} not found.");

        return MapToAdminResponse(partRequest);
    }

    public async Task<AdminPartRequestResponse> UpdatePartRequestStatusAsync(
        int partRequestId,
        UpdatePartRequestStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var partRequest = await _repository.GetPartRequestByIdAsync(partRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Part request with ID {partRequestId} not found.");

        if (!Enum.TryParse<PartRequestStatus>(request.Status.Trim(), true, out var newStatus))
        {
            throw new ArgumentException($"Invalid part request status: {request.Status}");
        }

        partRequest.Status = newStatus;
        await _repository.SaveChangesAsync(cancellationToken);

        return MapToAdminResponse(partRequest);
    }

    private static AdminPartRequestResponse MapToAdminResponse(PartRequest partRequest)
    {
        return new AdminPartRequestResponse
        {
            PartRequestId = partRequest.PartRequestId,
            CustomerId = partRequest.CustomerId,
            CustomerName = partRequest.Customer?.User?.FullName ?? string.Empty,
            CustomerEmail = partRequest.Customer?.User?.Email ?? string.Empty,
            CustomerPhone = partRequest.Customer?.User?.PhoneNumber ?? string.Empty,
            VehicleId = partRequest.VehicleId,
            VehicleNumber = partRequest.Vehicle?.VehicleNumber,
            VehicleMake = partRequest.Vehicle?.Make,
            VehicleModel = partRequest.Vehicle?.Model,
            VehicleManufactureYear = partRequest.Vehicle?.ManufactureYear,
            VehiclePhotoUrl = partRequest.Vehicle?.VehiclePhotoUrl,
            RequestedPartName = partRequest.RequestedPartName,
            Quantity = partRequest.Quantity,
            Details = partRequest.Details,
            PhotoUrl = partRequest.PhotoUrl,
            Status = partRequest.Status.ToString(),
            RequestDate = partRequest.RequestDate
        };
    }
}
