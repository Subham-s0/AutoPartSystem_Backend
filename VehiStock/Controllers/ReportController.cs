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
            if (date == default)
            {
                return BadRequest(new { message = "Invalid date" });
            }

            try
            {
                var result = await _reportService.GetDailyReport(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch daily report",
                    error = ex.Message
                });
            }
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthly([FromQuery] int year, [FromQuery] int month)
        {
            if (year <= 0 || month < 1 || month > 12)
            {
                return BadRequest(new { message = "Invalid year or month" });
            }

            try
            {
                var result = await _reportService.GetMonthlyReport(year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch monthly report",
                    error = ex.Message
                });
            }
        }

        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearly([FromQuery] int year)
        {
            if (year <= 0)
            {
                return BadRequest(new { message = "Invalid year" });
            }

            try
            {
                var result = await _reportService.GetYearlyReport(year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch yearly report",
                    error = ex.Message
                });
            }
        }
    }
}