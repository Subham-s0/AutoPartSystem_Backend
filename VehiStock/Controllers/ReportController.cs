using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Interfaces.IServices;

namespace VehiStock.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDaily([FromQuery] DateTime date)
        {
            var result = await _reportService.GetDailyReport(date);
            return Ok(result);
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthly(int year, int month)
        {
            var result = await _reportService.GetMonthlyReport(year, month);
            return Ok(result);
        }

        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearly(int year)
        {
            var result = await _reportService.GetYearlyReport(year);
            return Ok(result);
        }
    }
}