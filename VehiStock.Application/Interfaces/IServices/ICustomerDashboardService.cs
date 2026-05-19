using System.Threading;
using System.Threading.Tasks;
using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface ICustomerDashboardService
{
    Task<CustomerDashboardResponse> GetDashboardSummaryAsync(string userId, CancellationToken cancellationToken = default);
}
