using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface ICustomerDashboardRepository
{
    Task<int> GetCustomerIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetActiveVehiclesCountAsync(int customerId, CancellationToken cancellationToken = default);
    Task<int> GetUpcomingAppointmentsCountAsync(int customerId, CancellationToken cancellationToken = default);
    Task<decimal> GetOutstandingBalanceAsync(int customerId, CancellationToken cancellationToken = default);
    Task<int> GetPendingPartRequestsCountAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MonthlySpendingDto>> GetSpendingTrendAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecentActivityDto>> GetRecentActivitiesAsync(int customerId, CancellationToken cancellationToken = default);
}
