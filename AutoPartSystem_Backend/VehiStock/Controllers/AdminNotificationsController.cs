using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Notifications;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
[Route("api/admin/notifications")]
public class AdminNotificationsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AdminNotificationsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<NotificationResponse>>>> GetNotifications(
        [FromQuery] NotificationQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 50);
        var notifications = await _alertService.GetNotificationsAsync(GetCurrentUserId(), request, cancellationToken);
        return Ok(ApiResponse<PaginatedResponse<NotificationResponse>>.Ok(notifications, "Notifications fetched successfully."));
    }

    [HttpPatch("{notificationId:int}/read")]
    public async Task<ActionResult<ApiResponse<string>>> MarkAsRead(int notificationId, CancellationToken cancellationToken)
    {
        var marked = await _alertService.MarkNotificationAsReadAsync(GetCurrentUserId(), notificationId, cancellationToken);
        if (!marked)
        {
            return NotFound(ApiResponse<string>.Fail("Notification not found."));
        }

        return Ok(ApiResponse<string>.Ok("Notification marked as read.", "Notification marked as read."));
    }

    [HttpPost("process")]
    public async Task<ActionResult<ApiResponse<string>>> ProcessAlerts(CancellationToken cancellationToken)
    {
        await _alertService.ProcessAlertsAsync(cancellationToken);
        return Ok(ApiResponse<string>.Ok("Alert processing completed.", "Alert processing completed."));
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Authenticated user ID is missing.");
        }

        return userId;
    }
}
