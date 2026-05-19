using System.Threading;
using System.Threading.Tasks;
using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IServices;

public interface IStaffDashboardService
{
    Task<StaffDashboardResponse> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
}
