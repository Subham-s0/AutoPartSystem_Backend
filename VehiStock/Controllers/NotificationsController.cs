using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Notifications;
using VehiStock.Application.Interfaces.IServices;

namespace VehiStock.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public NotificationsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<NotificationResponse>>>> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _alertService.GetNotificationsAsync(GetCurrentUserId(), pageNumber, pageSize, cancellationToken);
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
