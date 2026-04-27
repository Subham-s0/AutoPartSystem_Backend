using VehiStock.Application.DTOs.Reports;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;

namespace VehiStock.Infrastructure.Services;

// Implementation for customer report generation
public class CustomerReportService(ICustomerReportRepository customerReportRepository) : ICustomerReportService
{
    public Task<IReadOnlyList<RegularCustomerReportItemDto>> GetRegularCustomersAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default) =>
        customerReportRepository.GetRegularCustomersAsync(filter, cancellationToken);

    public Task<IReadOnlyList<HighSpenderReportItemDto>> GetHighSpendersAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default) =>
        customerReportRepository.GetHighSpendersAsync(filter, cancellationToken);

    public Task<IReadOnlyList<PendingCreditReportItemDto>> GetPendingCreditsAsync(CustomerReportFilterDto filter, CancellationToken cancellationToken = default) =>
        customerReportRepository.GetPendingCreditsAsync(filter, cancellationToken);
}
