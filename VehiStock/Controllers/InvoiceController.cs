using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Invoices;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Infrastructure.Services;

namespace VehiStock.API.Controllers
{
    [ApiController]
    [Route("api/invoice")]
    public class InvoiceController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly InvoiceTemplateService _templateService;

        public InvoiceController(
            IEmailService emailService,
            InvoiceTemplateService templateService)
        {
            _emailService = emailService;
            _templateService = templateService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendInvoice([FromBody] InvoiceEmailDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is empty");

            if (string.IsNullOrWhiteSpace(dto.CustomerEmail))
                return BadRequest("Customer email is required");

            if (string.IsNullOrWhiteSpace(dto.InvoiceNumber))
                return BadRequest("Invoice number is required");

            try
            {
                var html = _templateService.Generate(
                    dto.CustomerName ?? "Customer",
                    dto.InvoiceNumber,
                    dto.TotalAmount
                );

                await _emailService.SendInvoiceEmail(
                    dto.CustomerEmail.Trim(),
                    "VehiStock Invoice",
                    html
                );

                return Ok(new
                {
                    success = true,
                    message = "Invoice sent successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}