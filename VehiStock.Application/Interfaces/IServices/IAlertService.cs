using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Notifications;

namespace VehiStock.Application.Interfaces.IServices;

public interface IAlertService
{
    Task ProcessAlertsAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResponse<NotificationResponse>> GetNotificationsAsync(
        string userId,
        NotificationQueryRequest request,
        CancellationToken cancellationToken = default);
    Task<bool> MarkNotificationAsReadAsync(string userId, int notificationId, CancellationToken cancellationToken = default);
}
