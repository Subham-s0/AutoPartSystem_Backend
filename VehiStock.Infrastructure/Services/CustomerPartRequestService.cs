using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class CustomerPartRequestService : ICustomerPartRequestService
{
    private readonly ICustomerPartRequestRepository _partRequestRepository;
    private readonly IImageStorageService _imageStorageService;

    public CustomerPartRequestService(
        ICustomerPartRequestRepository partRequestRepository,
        IImageStorageService imageStorageService)
    {
        _partRequestRepository = partRequestRepository;
        _imageStorageService = imageStorageService;
    }

    public async Task<PartRequestResponse> CreatePartRequestAsync(
        string userId,
        CreatePartRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        Vehicle? vehicle = null;
        if (request.VehicleId.HasValue)
        {
            vehicle = await _partRequestRepository.GetVehicleForCustomerAsync(
                customer.CustomerId,
                request.VehicleId.Value,
                cancellationToken);
            if (vehicle is null)
            {
                throw new InvalidOperationException("Vehicle not found for this customer.");
            }
        }

        string? photoUrl = null;
        if (request.Photo != null)
        {
            // Uploaded file takes priority
            photoUrl = await _imageStorageService.SaveImageAsync(request.Photo, "part-requests", cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.PhotoUrl))
        {
            // Customer provided a direct URL
            photoUrl = request.PhotoUrl.Trim();
        }

        var partRequest = new PartRequest
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle?.VehicleId,
            RequestedPartName = request.RequestedPartName.Trim(),
            Quantity = request.Quantity,
            Details = string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim(),
            PhotoUrl = photoUrl,
            Status = PartRequestStatus.Pending
        };

        var created = await _partRequestRepository.CreatePartRequestAsync(partRequest, cancellationToken);
        return MapPartRequest(created);
    }

    public async Task<PaginatedResponse<PartRequestResponse>> GetPartRequestsPageAsync(
        string userId,
        PartRequestQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var normalizedQuery = NormalizePartRequestQuery(query);
        var page = await _partRequestRepository.GetPartRequestsPageAsync(
            customer.CustomerId,
            normalizedQuery,
            cancellationToken);

        return new PaginatedResponse<PartRequestResponse>
        {
            Items = page.Items.Select(MapPartRequest).ToList(),
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalRecords = page.TotalRecords,
            TotalPages = page.TotalPages
        };
    }

    public async Task<PartRequestResponse> CancelPartRequestAsync(
        string userId,
        int partRequestId,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var partRequest = await _partRequestRepository.GetPartRequestForCustomerAsync(
            customer.CustomerId,
            partRequestId,
            cancellationToken);

        if (partRequest is null)
        {
            throw new InvalidOperationException("Part request was not found.");
        }

        if (partRequest.Status == PartRequestStatus.Cancelled)
        {
            throw new InvalidOperationException("This part request is already cancelled.");
        }

        if (partRequest.Status != PartRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Only pending part requests can be cancelled. This request is {partRequest.Status}.");
        }

        partRequest.Status = PartRequestStatus.Cancelled;
        await _partRequestRepository.SaveChangesAsync(cancellationToken);

        return MapPartRequest(partRequest);
    }

    private async Task<CustomerProfile> GetCustomerProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var customer = await _partRequestRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer profile was not found for this account.");
        }

        return customer;
    }

    private static PartRequestResponse MapPartRequest(PartRequest partRequest)
    {
        return new PartRequestResponse
        {
            PartRequestId = partRequest.PartRequestId,
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

    private static PartRequestQueryRequest NormalizePartRequestQuery(PartRequestQueryRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            !Enum.TryParse<PartRequestStatus>(request.Status.Trim(), true, out _))
        {
            throw new InvalidOperationException("Invalid part request status.");
        }

        return new PartRequestQueryRequest
        {
            PageNumber = Math.Max(1, request.PageNumber),
            PageSize = Math.Clamp(request.PageSize, 1, 100),
            SearchText = string.IsNullOrWhiteSpace(request.SearchText) ? null : request.SearchText.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim(),
            Sorts = request.Sorts
        };
    }
}
