using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;

namespace VehiStock.Infrastructure.Services;

// Implementation for customer report generation
public class StaffReportService : IStaffReportService
{
    private readonly IStaffReportRepository _staffReportRepository;

    public StaffReportService(IStaffReportRepository staffReportRepository)
    {
        _staffReportRepository = staffReportRepository;
    }

    public async Task<PaginatedResponse<RegularCustomerReportResponse>> GetRegularCustomersAsync(int pageNumber, int pageSize, int minimumInvoices, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = Math.Max(1, pageNumber);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);
        var normalizedMinimumInvoices = Math.Max(1, minimumInvoices);
        var (items, totalRecords) = await _staffReportRepository.GetRegularCustomersAsync(normalizedPageNumber, normalizedPageSize, normalizedMinimumInvoices, fromDate, toDate, cancellationToken);
        return CreatePage(items, normalizedPageNumber, normalizedPageSize, totalRecords);
    }

    public async Task<PaginatedResponse<HighSpenderReportResponse>> GetHighSpendersAsync(int pageNumber, int pageSize, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = Math.Max(1, pageNumber);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);
        var (items, totalRecords) = await _staffReportRepository.GetHighSpendersAsync(normalizedPageNumber, normalizedPageSize, fromDate, toDate, cancellationToken);
        return CreatePage(items, normalizedPageNumber, normalizedPageSize, totalRecords);
    }

    public async Task<PaginatedResponse<PendingCreditReportResponse>> GetPendingCreditsAsync(int pageNumber, int pageSize, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = Math.Max(1, pageNumber);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);
        var (items, totalRecords) = await _staffReportRepository.GetPendingCreditsAsync(normalizedPageNumber, normalizedPageSize, fromDate, toDate, cancellationToken);
        return CreatePage(items, normalizedPageNumber, normalizedPageSize, totalRecords);
    }

    public Task<CustomerReportSummaryResponse> GetSummaryAsync(DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        return _staffReportRepository.GetSummaryAsync(fromDate, toDate, cancellationToken);
    }

    private static PaginatedResponse<T> CreatePage<T>(IReadOnlyCollection<T> items, int pageNumber, int pageSize, int totalRecords)
    {
        return new PaginatedResponse<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
    }
}
