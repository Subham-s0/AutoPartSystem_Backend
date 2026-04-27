using VehiStock.Application.DTOs.Reports;

namespace VehiStock.Application.Interfaces.IServices;

public interface ICustomerReportService
{
    Task<IReadOnlyList<RegularCustomerReportItemDto>> GetRegularCustomersAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HighSpenderReportItemDto>> GetHighSpendersAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PendingCreditReportItemDto>> GetPendingCreditsAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default);
}
