using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Reports;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers
{
    [ApiController]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("api/admin/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("daily")]
        public async Task<ActionResult<ApiResponse<Reports>>> GetDailyReport([FromQuery] DateTime? date)
        {
            var targetDate = date ?? DateTime.UtcNow;
            var result = await _reportService.GetDailyReport(targetDate);
            return Ok(ApiResponse<Reports>.Ok(result));
        }

        [HttpGet("monthly")]
        public async Task<ActionResult<ApiResponse<Reports>>> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            var result = await _reportService.GetMonthlyReport(year, month);
            return Ok(ApiResponse<Reports>.Ok(result));
        }

        [HttpGet("yearly")]
        public async Task<ActionResult<ApiResponse<Reports>>> GetYearlyReport([FromQuery] int year)
        {
            var result = await _reportService.GetYearlyReport(year);
            return Ok(ApiResponse<Reports>.Ok(result));
        }
    }
}