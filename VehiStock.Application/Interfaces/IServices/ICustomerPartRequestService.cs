using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface ICustomerPartRequestService
{
    Task<PartRequestResponse> CreatePartRequestAsync(
        string userId,
        CreatePartRequestRequest request,
        CancellationToken cancellationToken = default);

    Task<PaginatedResponse<PartRequestResponse>> GetPartRequestsPageAsync(
        string userId,
        PartRequestQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<PartRequestResponse> CancelPartRequestAsync(
        string userId,
        int partRequestId,
        CancellationToken cancellationToken = default);
}
