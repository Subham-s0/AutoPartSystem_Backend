using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Notifications;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AlertRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Part>> GetLowStockPartsAsync(int defaultThreshold, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Parts
            .Where(x => x.IsActive && x.StockQty < defaultThreshold)
            .OrderBy(x => x.StockQty)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SalesInvoice>> GetOverdueCreditInvoicesAsync(DateOnly overdueBefore, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SalesInvoices
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .Include(x => x.Vehicle)
            .Where(x =>
                x.BalanceDue > 0 &&
                x.CreditDueDate.HasValue &&
                x.CreditDueDate.Value <= overdueBefore &&
                (x.PaymentStatus == PaymentStatus.Unpaid ||
                 x.PaymentStatus == PaymentStatus.Partial ||
                 x.PaymentStatus == PaymentStatus.Overdue))
            .OrderBy(x => x.CreditDueDate)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasRecentNotificationAsync(
        string userId,
        NotificationType notificationType,
        string referenceType,
        int referenceId,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications.AnyAsync(
            x => x.UserId == userId &&
                 x.NotificationType == notificationType &&
                 x.ReferenceType == referenceType &&
                 x.ReferenceId == referenceId &&
                 x.CreatedAt >= sinceUtc,
            cancellationToken);
    }

    public async Task AddNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedResponse<Notification>> GetNotificationsForUserAsync(
        string userId,
        NotificationQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var normalizedPageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 50);

        var query = _dbContext.Notifications
            .Where(x => x.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.NotificationType) &&
            Enum.TryParse<NotificationType>(request.NotificationType.Trim(), true, out var notificationType))
        {
            query = query.Where(x => x.NotificationType == notificationType);
        }

        if (request.IsRead.HasValue)
        {
            query = query.Where(x => x.IsRead == request.IsRead.Value);
        }

        if (request.FromDate.HasValue)
        {
            var fromUtc = request.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreatedAt >= fromUtc);
        }

        if (request.ToDate.HasValue)
        {
            var toUtc = request.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreatedAt <= toUtc);
        }

        query = query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.NotificationId);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<Notification>
        {
            Items = items,
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)normalizedPageSize)
        };
    }

    public Task<Notification?> GetNotificationForUserAsync(int notificationId, string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications
            .SingleOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
