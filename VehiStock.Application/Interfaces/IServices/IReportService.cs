using VehiStock.Application.Dtos.Reports;

namespace VehiStock.Application.Interfaces.IServices
{
    public interface IReportService
    {
        Task<Reports> GetDailyReport(DateTime date);
        Task<Reports> GetMonthlyReport(int year, int month);
        Task<Reports> GetYearlyReport(int year);
    }
}