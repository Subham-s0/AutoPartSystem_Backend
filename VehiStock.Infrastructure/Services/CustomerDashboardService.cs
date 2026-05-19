using System.Threading;
using System.Threading.Tasks;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;

namespace VehiStock.Infrastructure.Services;

public class CustomerDashboardService : ICustomerDashboardService
{
    private readonly ICustomerDashboardRepository _repository;

    public CustomerDashboardService(ICustomerDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDashboardResponse> GetDashboardSummaryAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var customerId = await _repository.GetCustomerIdByUserIdAsync(userId, cancellationToken);

        var activeVehiclesCount = await _repository.GetActiveVehiclesCountAsync(customerId, cancellationToken);
        var upcomingAppointmentsCount = await _repository.GetUpcomingAppointmentsCountAsync(customerId, cancellationToken);
        var outstandingBalance = await _repository.GetOutstandingBalanceAsync(customerId, cancellationToken);
        var pendingPartRequestsCount = await _repository.GetPendingPartRequestsCountAsync(customerId, cancellationToken);

        var spendingTrend = await _repository.GetSpendingTrendAsync(customerId, cancellationToken);
        var recentActivities = await _repository.GetRecentActivitiesAsync(customerId, cancellationToken);

        return new CustomerDashboardResponse
        {
            Kpis = new CustomerDashboardKpis
            {
                ActiveVehiclesCount = activeVehiclesCount,
                UpcomingAppointmentsCount = upcomingAppointmentsCount,
                OutstandingBalance = outstandingBalance,
                PendingPartRequestsCount = pendingPartRequestsCount,
            },
            SpendingTrend = spendingTrend,
            RecentActivities = recentActivities,
        };
    }
}
