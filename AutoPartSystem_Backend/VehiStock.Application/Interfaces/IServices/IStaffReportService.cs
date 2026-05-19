using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IServices;

// Used for staff report management
public interface IStaffReportService
{
    Task<PaginatedResponse<RegularCustomerReportResponse>> GetRegularCustomersAsync(int pageNumber, int pageSize, int minimumInvoices, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<HighSpenderReportResponse>> GetHighSpendersAsync(int pageNumber, int pageSize, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<PendingCreditReportResponse>> GetPendingCreditsAsync(int pageNumber, int pageSize, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default);
    Task<CustomerReportSummaryResponse> GetSummaryAsync(DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default);
}
