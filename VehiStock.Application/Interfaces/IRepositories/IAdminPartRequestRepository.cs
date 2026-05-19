using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IAdminPartRequestRepository
{
    Task<PaginatedResponse<PartRequest>> GetPartRequestsPageAsync(
        AdminPartRequestQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<PartRequest?> GetPartRequestByIdAsync(
        int partRequestId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
