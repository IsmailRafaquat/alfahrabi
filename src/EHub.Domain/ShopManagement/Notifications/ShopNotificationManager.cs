using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace EHub.ShopManagement.Notifications;

/// <summary>
/// Central write-path for notifications: creates/updates by SourceKey (never duplicates an active
/// occurrence of the same condition), resolves conditions that are no longer true, and applies
/// read/unread/dismiss state either directly on a user-specific notification or via
/// <see cref="ShopNotificationUserState"/> for a tenant-wide one.
/// </summary>
public class ShopNotificationManager : DomainService
{
    private readonly IRepository<ShopNotification, Guid> _notificationRepository;
    private readonly IRepository<ShopNotificationUserState, Guid> _userStateRepository;

    public ShopNotificationManager(
        IRepository<ShopNotification, Guid> notificationRepository,
        IRepository<ShopNotificationUserState, Guid> userStateRepository)
    {
        _notificationRepository = notificationRepository;
        _userStateRepository = userStateRepository;
    }

    public async Task<ShopNotification> CreateOrUpdateAsync(
        Guid tenantId,
        ShopNotificationType type,
        ShopNotificationSeverity severity,
        string title,
        string message,
        string sourceKey,
        Guid? userId = null,
        string? referenceType = null,
        Guid? referenceId = null,
        string? referenceNumber = null,
        string? navigationUrl = null,
        string? actionLabel = null,
        string? sourceDataJson = null,
        DateTime? expiresDate = null)
    {
        var now = Clock.Now;
        var active = await _notificationRepository.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.SourceKey == sourceKey && x.Status != ShopNotificationStatus.Resolved);

        if (active != null)
        {
            active.UpdateContent(severity, title, message, referenceType, referenceId, referenceNumber, navigationUrl, actionLabel, sourceDataJson, now, expiresDate);
            await _notificationRepository.UpdateAsync(active);
            return active;
        }

        // No active row for this SourceKey - either it's brand new, or the previous occurrence was
        // already Resolved. Either way, insert a fresh row; the filtered unique index on
        // (TenantId, SourceKey) WHERE Status <> Resolved keeps this from ever duplicating an
        // in-flight occurrence while leaving prior Resolved rows untouched as history.
        var notification = new ShopNotification(
            GuidGenerator.Create(), tenantId, type, severity, title, message, sourceKey, now,
            userId, referenceType, referenceId, referenceNumber, navigationUrl, actionLabel, sourceDataJson, expiresDate);

        await _notificationRepository.InsertAsync(notification);
        return notification;
    }

    /// <summary>
    /// For one-shot historical events (a specific cash closing's difference, a specific draft's
    /// pending-too-long check) rather than an ongoing condition - true once any notification (in any
    /// status) has ever been recorded for this exact SourceKey, so a background sweep never
    /// resurrects/duplicates something the user already saw and dismissed or that was resolved.
    /// </summary>
    public async Task<bool> ExistsBySourceKeyAsync(Guid tenantId, string sourceKey)
    {
        return await _notificationRepository.AnyAsync(x => x.TenantId == tenantId && x.SourceKey == sourceKey);
    }

    public async Task ResolveBySourceKeyAsync(Guid tenantId, string sourceKey)
    {
        var active = await _notificationRepository.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.SourceKey == sourceKey && x.Status != ShopNotificationStatus.Resolved);
        if (active == null) return;

        active.Resolve(Clock.Now);
        await _notificationRepository.UpdateAsync(active);
    }

    /// <summary>
    /// Resolves every still-active notification of <paramref name="type"/> whose ReferenceId is not
    /// in <paramref name="stillActiveReferenceIds"/> - the sweep half of a full-tenant generator run
    /// (e.g. a product that was Low Stock last run but has since been restocked).
    /// </summary>
    public async Task ResolveStaleAsync(Guid tenantId, ShopNotificationType type, ISet<Guid> stillActiveReferenceIds)
    {
        var active = await _notificationRepository.GetListAsync(
            x => x.TenantId == tenantId && x.Type == type && x.Status != ShopNotificationStatus.Resolved);

        foreach (var notification in active)
        {
            if (notification.ReferenceId.HasValue && !stillActiveReferenceIds.Contains(notification.ReferenceId.Value))
            {
                notification.Resolve(Clock.Now);
                await _notificationRepository.UpdateAsync(notification);
            }
        }
    }

    public async Task MarkAsReadAsync(Guid tenantId, Guid notificationId, Guid userId)
    {
        var notification = await GetVisibleAsync(tenantId, notificationId, userId);

        if (notification.UserId.HasValue)
        {
            notification.MarkAsRead(Clock.Now);
            await _notificationRepository.UpdateAsync(notification);
        }
        else
        {
            await UpsertUserStateAsync(tenantId, notificationId, userId, state => state.MarkAsRead(Clock.Now));
        }
    }

    public async Task MarkAsUnreadAsync(Guid tenantId, Guid notificationId, Guid userId)
    {
        var notification = await GetVisibleAsync(tenantId, notificationId, userId);

        if (notification.UserId.HasValue)
        {
            notification.MarkAsUnread();
            await _notificationRepository.UpdateAsync(notification);
        }
        else
        {
            await UpsertUserStateAsync(tenantId, notificationId, userId, state => state.MarkAsUnread());
        }
    }

    public async Task DismissAsync(Guid tenantId, Guid notificationId, Guid userId)
    {
        var notification = await GetVisibleAsync(tenantId, notificationId, userId);

        if (notification.UserId.HasValue)
        {
            notification.Dismiss(Clock.Now);
            await _notificationRepository.UpdateAsync(notification);
        }
        else
        {
            await UpsertUserStateAsync(tenantId, notificationId, userId, state => state.Dismiss(Clock.Now));
        }
    }

    public async Task DeleteAsync(Guid tenantId, Guid notificationId, Guid userId)
    {
        var notification = await GetVisibleAsync(tenantId, notificationId, userId);
        await _notificationRepository.DeleteAsync(notification);
    }

    public async Task DeleteExpiredNotificationsAsync(Guid tenantId, int retentionDays)
    {
        var now = Clock.Now;
        var cutoff = now.AddDays(-retentionDays);
        var expired = await _notificationRepository.GetListAsync(x =>
            x.TenantId == tenantId &&
            ((x.ExpiresDate.HasValue && x.ExpiresDate.Value < now) ||
             (x.Status == ShopNotificationStatus.Resolved && x.ResolvedDate.HasValue && x.ResolvedDate.Value < cutoff) ||
             (x.Status == ShopNotificationStatus.Dismissed && x.DismissedDate.HasValue && x.DismissedDate.Value < cutoff)));

        if (expired.Count == 0) return;
        await _notificationRepository.DeleteManyAsync(expired);
    }

    private async Task<ShopNotification> GetVisibleAsync(Guid tenantId, Guid notificationId, Guid userId)
    {
        var notification = await _notificationRepository.FindAsync(notificationId);
        if (notification == null || notification.TenantId != tenantId)
            throw new BusinessException("ShopManagement:NotificationNotFound");
        if (notification.UserId.HasValue && notification.UserId.Value != userId)
            throw new BusinessException("ShopManagement:NotificationNotFound");

        return notification;
    }

    private async Task UpsertUserStateAsync(Guid tenantId, Guid notificationId, Guid userId, Action<ShopNotificationUserState> apply)
    {
        var state = await _userStateRepository.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.NotificationId == notificationId && x.UserId == userId);

        if (state == null)
        {
            state = new ShopNotificationUserState(GuidGenerator.Create(), tenantId, notificationId, userId);
            apply(state);
            await _userStateRepository.InsertAsync(state);
        }
        else
        {
            apply(state);
            await _userStateRepository.UpdateAsync(state);
        }
    }
}
