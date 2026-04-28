using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Interfaces.IServices;

namespace VehiStock.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport(DateTime from, DateTime to)
    {
        var result = await _reportService.GetSalesReport(from, to);
        return Ok(result);
    }

    [HttpGet("purchase")]
    public async Task<IActionResult> GetPurchaseReport(DateTime from, DateTime to)
    {
        var result = await _reportService.GetPurchaseReport(from, to);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _reportService.GetDashboardReport();
        return Ok(result);
    }
}