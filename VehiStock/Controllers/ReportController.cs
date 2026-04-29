using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Interfaces.IServices;

namespace VehiStock.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyReport([FromQuery] DateTime date)
        {
            var result = await _reportService.GetDailyReport(date);
            return Ok(result);
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            var result = await _reportService.GetMonthlyReport(year, month);
            return Ok(result);
        }

        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearlyReport([FromQuery] int year)
        {
            var result = await _reportService.GetYearlyReport(year);
            return Ok(result);
        }
    }
}