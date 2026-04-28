using VehiStock.Application.Dtos.Common;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IAlertRepository
{
    Task<IReadOnlyCollection<Part>> GetLowStockPartsAsync(int defaultThreshold, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesInvoice>> GetOverdueCreditInvoicesAsync(DateOnly overdueBefore, CancellationToken cancellationToken = default);
    Task<bool> HasRecentNotificationAsync(
        string userId,
        NotificationType notificationType,
        string referenceType,
        int referenceId,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default);
    Task AddNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Notification>> GetNotificationsForUserAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Notification?> GetNotificationForUserAsync(int notificationId, string userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
