using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Infrastructure.Services;
using VehiStock.Application.Dtos.Invoices;

namespace VehiStock.API.Controllers
{
    [ApiController]
    [Route("api/invoice")]
    public class InvoiceController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly InvoiceTemplateService _templateService;

        public InvoiceController(IEmailService emailService, InvoiceTemplateService templateService)
        {
            _emailService = emailService;
            _templateService = templateService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendInvoice([FromBody] InvoiceEmailDto dto)
        {
            var html = _templateService.Generate(
                dto.CustomerName,
                dto.InvoiceNumber,
                dto.TotalAmount
            );

            await _emailService.SendInvoiceEmail(
                dto.CustomerEmail,
                "VehiStock Invoice",
                html
            );

            return Ok(new { message = "Invoice sent successfully" });
        }
    }
}