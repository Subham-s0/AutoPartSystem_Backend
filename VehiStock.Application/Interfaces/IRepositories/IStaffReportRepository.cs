using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IRepositories;

// Used for staff report data access
public interface IStaffReportRepository
{
    Task<(IReadOnlyCollection<RegularCustomerReportResponse> Items, int TotalRecords)> GetRegularCustomersAsync(int pageNumber, int pageSize, int minimumInvoices, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<HighSpenderReportResponse> Items, int TotalRecords)> GetHighSpendersAsync(int pageNumber, int pageSize, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<PendingCreditReportResponse> Items, int TotalRecords)> GetPendingCreditsAsync(int pageNumber, int pageSize, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default);
    Task<CustomerReportSummaryResponse> GetSummaryAsync(DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default);
}
