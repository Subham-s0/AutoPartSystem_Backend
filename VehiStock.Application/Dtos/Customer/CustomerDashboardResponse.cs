namespace VehiStock.Application.Dtos.Customer;

public class CustomerDashboardResponse
{
    public CustomerDashboardKpis Kpis { get; set; } = new();
    public IReadOnlyCollection<MonthlySpendingDto> SpendingTrend { get; set; } = Array.Empty<MonthlySpendingDto>();
    public IReadOnlyCollection<RecentActivityDto> RecentActivities { get; set; } = Array.Empty<RecentActivityDto>();
}

public class CustomerDashboardKpis
{
    public int ActiveVehiclesCount { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int PendingPartRequestsCount { get; set; }
}

public class MonthlySpendingDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
