using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Interfaces.IServices;

public interface IAdminPartRequestService
{
    Task<PaginatedResponse<AdminPartRequestResponse>> GetPartRequestsPageAsync(
        AdminPartRequestQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<AdminPartRequestResponse> GetPartRequestByIdAsync(
        int partRequestId,
        CancellationToken cancellationToken = default);

    Task<AdminPartRequestResponse> UpdatePartRequestStatusAsync(
        int partRequestId,
        UpdatePartRequestStatusRequest request,
        CancellationToken cancellationToken = default);
}
