using VehiStock.Application.DTOs.Analytics;

namespace VehiStock.Application.Interfaces.IServices
{
    public interface IAnalyticsService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }
}