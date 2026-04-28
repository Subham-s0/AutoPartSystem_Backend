using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Notifications;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using VehiStock.Infrastructure.Settings;

namespace VehiStock.Infrastructure.Services;

public class AlertService : IAlertService
{
    private const string PartReferenceType = "Part";
    private const string SalesInvoiceReferenceType = "SalesInvoice";

    private readonly IAlertRepository _alertRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AlertProcessingSettings _alertSettings;

    public AlertService(
        IAlertRepository alertRepository,
        UserManager<ApplicationUser> userManager,
        IOptions<AlertProcessingSettings> alertOptions)
    {
        _alertRepository = alertRepository;
        _userManager = userManager;
        _alertSettings = alertOptions.Value;
    }

    public async Task ProcessAlertsAsync(CancellationToken cancellationToken = default)
    {
        await ProcessLowStockNotificationsAsync(cancellationToken);
        await ProcessOverdueCreditRemindersAsync(cancellationToken);
    }

    public async Task<PaginatedResponse<NotificationResponse>> GetNotificationsAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var paginatedNotifications = await _alertRepository.GetNotificationsForUserAsync(userId, pageNumber, pageSize, cancellationToken);
        return new PaginatedResponse<NotificationResponse>
        {
            Items = paginatedNotifications.Items.Select(MapNotification).ToArray(),
            PageNumber = paginatedNotifications.PageNumber,
            PageSize = paginatedNotifications.PageSize,
            TotalRecords = paginatedNotifications.TotalRecords,
            TotalPages = paginatedNotifications.TotalPages
        };
    }

    public async Task<bool> MarkNotificationAsReadAsync(string userId, int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _alertRepository.GetNotificationForUserAsync(notificationId, userId, cancellationToken);
        if (notification is null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _alertRepository.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private async Task ProcessLowStockNotificationsAsync(CancellationToken cancellationToken)
    {
        var lowStockParts = await _alertRepository.GetLowStockPartsAsync(_alertSettings.LowStockThreshold, cancellationToken);
        if (lowStockParts.Count == 0)
        {
            return;
        }

        var adminUsers = await _userManager.GetUsersInRoleAsync(RoleNames.Admin);
        if (adminUsers.Count == 0)
        {
            return;
        }

        var sinceUtc = DateTime.UtcNow.AddHours(-Math.Max(1, _alertSettings.NotificationRepeatHours));

        foreach (var part in lowStockParts)
        {
            foreach (var adminUser in adminUsers)
            {
                var hasRecentNotification = await _alertRepository.HasRecentNotificationAsync(
                    adminUser.Id,
                    NotificationType.LowStock,
                    PartReferenceType,
                    part.PartId,
                    sinceUtc,
                    cancellationToken);

                if (hasRecentNotification)
                {
                    continue;
                }

                await _alertRepository.AddNotificationAsync(new Notification
                {
                    UserId = adminUser.Id,
                    NotificationType = NotificationType.LowStock,
                    Title = $"Low stock: {part.PartName}",
                    Message = $"{part.PartName} ({part.Brand}) has only {part.StockQty} units remaining.",
                    ReferenceType = PartReferenceType,
                    ReferenceId = part.PartId
                }, cancellationToken);
            }
        }
    }

    private async Task ProcessOverdueCreditRemindersAsync(CancellationToken cancellationToken)
    {
        var overdueBefore = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddMonths(-1);
        var overdueInvoices = await _alertRepository.GetOverdueCreditInvoicesAsync(overdueBefore, cancellationToken);
        if (overdueInvoices.Count == 0)
        {
            return;
        }

        var sinceUtc = DateTime.UtcNow.AddHours(-Math.Max(1, _alertSettings.NotificationRepeatHours));

        foreach (var invoice in overdueInvoices)
        {
            invoice.PaymentStatus = PaymentStatus.Overdue;

            var customerUser = invoice.Customer.User;
            if (string.IsNullOrWhiteSpace(customerUser.Email))
            {
                continue;
            }

            var hasRecentNotification = await _alertRepository.HasRecentNotificationAsync(
                customerUser.Id,
                NotificationType.CreditReminder,
                SalesInvoiceReferenceType,
                invoice.SalesInvoiceId,
                sinceUtc,
                cancellationToken);

            if (hasRecentNotification)
            {
                continue;
            }

            var title = $"Payment reminder for invoice {invoice.InvoiceNo}";
            var message = $"Your balance of {invoice.BalanceDue:0.00} for invoice {invoice.InvoiceNo} is overdue. Please settle the payment as soon as possible.";

            await _alertRepository.AddNotificationAsync(new Notification
            {
                UserId = customerUser.Id,
                NotificationType = NotificationType.CreditReminder,
                Title = title,
                Message = message,
                ReferenceType = SalesInvoiceReferenceType,
                ReferenceId = invoice.SalesInvoiceId
            }, cancellationToken);
        }

        await _alertRepository.SaveChangesAsync(cancellationToken);
    }

    private static NotificationResponse MapNotification(Notification notification)
    {
        return new NotificationResponse
        {
            NotificationId = notification.NotificationId,
            NotificationType = notification.NotificationType.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            ReferenceType = notification.ReferenceType,
            ReferenceId = notification.ReferenceId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }
}
