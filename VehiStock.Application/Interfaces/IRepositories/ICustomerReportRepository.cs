using VehiStock.Application.DTOs.Reports;

namespace VehiStock.Application.Interfaces.IRepositories;

// Used for customer report data access
public interface ICustomerReportRepository
{
    Task<IReadOnlyList<RegularCustomerReportItemDto>> GetRegularCustomersAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HighSpenderReportItemDto>> GetHighSpendersAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PendingCreditReportItemDto>> GetPendingCreditsAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default);
}
