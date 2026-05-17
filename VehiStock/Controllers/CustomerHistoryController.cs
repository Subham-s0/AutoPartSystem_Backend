using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Customer)]
[Route("api/customer/history")]
public class CustomerHistoryController : ControllerBase
{
    private readonly ICustomerHistoryService _customerHistoryService;

    public CustomerHistoryController(ICustomerHistoryService customerHistoryService)
    {
        _customerHistoryService = customerHistoryService;
    }

    [HttpGet("purchases")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PurchaseHistoryResponse>>>> GetPurchaseHistory(
        [FromQuery] PurchaseHistoryQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _customerHistoryService.GetPurchaseHistoryPageAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<PaginatedResponse<PurchaseHistoryResponse>>.Ok(history, "Purchase history fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaginatedResponse<PurchaseHistoryResponse>>.Fail(ex.Message));
        }
    }

    [HttpGet("purchases/{salesInvoiceId:int}")]
    public async Task<ActionResult<ApiResponse<PurchaseHistoryResponse>>> GetPurchaseHistoryDetail(
        int salesInvoiceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = await _customerHistoryService.GetPurchaseHistoryDetailAsync(
                GetCurrentUserId(),
                salesInvoiceId,
                cancellationToken);
            return Ok(ApiResponse<PurchaseHistoryResponse>.Ok(purchase, "Purchase invoice fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<PurchaseHistoryResponse>.Fail(ex.Message));
        }
    }

    [HttpGet("services")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ServiceHistoryResponse>>>> GetServiceHistory(
        [FromQuery] ServiceHistoryQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _customerHistoryService.GetServiceHistoryPageAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<PaginatedResponse<ServiceHistoryResponse>>.Ok(history, "Service history fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaginatedResponse<ServiceHistoryResponse>>.Fail(ex.Message));
        }
    }

    [HttpGet("services/{serviceRecordId:int}")]
    public async Task<ActionResult<ApiResponse<ServiceHistoryResponse>>> GetServiceHistoryDetail(
        int serviceRecordId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var detail = await _customerHistoryService.GetServiceHistoryDetailAsync(GetCurrentUserId(), serviceRecordId, cancellationToken);
            return Ok(ApiResponse<ServiceHistoryResponse>.Ok(detail, "Service record fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<ServiceHistoryResponse>.Fail(ex.Message));
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Authenticated user ID is missing.");
        return userId;
    }
}
