using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Notifications;

/// <summary>
/// Per-user read/dismiss state for a tenant-wide <see cref="ShopNotification"/> (UserId == null on
/// the notification itself). Used instead of mutating the shared notification, so marking it read
/// for one user does not affect what every other user in the tenant sees.
/// </summary>
public class ShopNotificationUserState : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid NotificationId { get; protected set; }
    public Guid UserId { get; protected set; }
    public ShopNotificationStatus Status { get; protected set; } = ShopNotificationStatus.Unread;
    public DateTime? ReadDate { get; protected set; }
    public DateTime? DismissedDate { get; protected set; }

    protected ShopNotificationUserState() { }

    internal ShopNotificationUserState(Guid id, Guid tenantId, Guid notificationId, Guid userId) : base(id)
    {
        TenantId = tenantId;
        NotificationId = notificationId;
        UserId = userId;
        Status = ShopNotificationStatus.Unread;
    }

    internal void MarkAsRead(DateTime date)
    {
        Status = ShopNotificationStatus.Read;
        ReadDate = date;
    }

    internal void MarkAsUnread()
    {
        Status = ShopNotificationStatus.Unread;
        ReadDate = null;
    }

    internal void Dismiss(DateTime date)
    {
        Status = ShopNotificationStatus.Dismissed;
        DismissedDate = date;
    }
}
