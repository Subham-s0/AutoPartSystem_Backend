using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface IPaymentService
{
    Task<PaginatedResponse<CustomerPaymentListResponse>> GetPaymentsPageAsync(string userId, CustomerPaymentQueryRequest request, CancellationToken cancellationToken = default);
}
