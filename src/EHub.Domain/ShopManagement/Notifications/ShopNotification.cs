using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Notifications;

public class ShopNotification : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    /// <summary>Null for a tenant-wide notification; set for a notification assigned to one user.</summary>
    public Guid? UserId { get; protected set; }

    public ShopNotificationType Type { get; protected set; }
    public ShopNotificationSeverity Severity { get; protected set; }
    public ShopNotificationStatus Status { get; protected set; } = ShopNotificationStatus.Unread;

    public string Title { get; protected set; } = string.Empty;
    public string Message { get; protected set; } = string.Empty;

    public string? ReferenceType { get; protected set; }
    public Guid? ReferenceId { get; protected set; }
    public string? ReferenceNumber { get; protected set; }

    /// <summary>Built server-side from an allowlist map keyed by ReferenceType - never accepted as raw input.</summary>
    public string? NavigationUrl { get; protected set; }
    public string? ActionLabel { get; protected set; }

    /// <summary>Stable identity of the underlying condition, e.g. "LowStock:{TenantId}:{ProductId}".</summary>
    public string SourceKey { get; protected set; } = string.Empty;
    public string? SourceDataJson { get; protected set; }

    public DateTime TriggeredDate { get; protected set; }
    public DateTime? ReadDate { get; protected set; }
    public DateTime? DismissedDate { get; protected set; }
    public DateTime? ResolvedDate { get; protected set; }
    public DateTime? ExpiresDate { get; protected set; }

    protected ShopNotification() { }

    internal ShopNotification(
        Guid id,
        Guid tenantId,
        ShopNotificationType type,
        ShopNotificationSeverity severity,
        string title,
        string message,
        string sourceKey,
        DateTime triggeredDate,
        Guid? userId,
        string? referenceType,
        Guid? referenceId,
        string? referenceNumber,
        string? navigationUrl,
        string? actionLabel,
        string? sourceDataJson,
        DateTime? expiresDate) : base(id)
    {
        TenantId = tenantId;
        Type = type;
        SourceKey = Check.NotNullOrWhiteSpace(sourceKey, nameof(sourceKey), ShopNotificationConsts.SourceKeyMaxLength);
        Status = ShopNotificationStatus.Unread;
        UserId = userId;
        UpdateContent(severity, title, message, referenceType, referenceId, referenceNumber, navigationUrl, actionLabel, sourceDataJson, triggeredDate, expiresDate);
    }

    /// <summary>
    /// Refreshes an existing active notification's content when the same condition (SourceKey) is
    /// re-evaluated - e.g. a Low Stock warning's current-stock figure changing - without creating a
    /// duplicate row and without disturbing its read/unread state.
    /// </summary>
    internal void UpdateContent(
        ShopNotificationSeverity severity,
        string title,
        string message,
        string? referenceType,
        Guid? referenceId,
        string? referenceNumber,
        string? navigationUrl,
        string? actionLabel,
        string? sourceDataJson,
        DateTime triggeredDate,
        DateTime? expiresDate)
    {
        Severity = severity;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), ShopNotificationConsts.TitleMaxLength);
        Message = Check.NotNullOrWhiteSpace(message, nameof(message), ShopNotificationConsts.MessageMaxLength);
        ReferenceType = Check.Length(referenceType, nameof(referenceType), ShopNotificationConsts.ReferenceTypeMaxLength);
        ReferenceId = referenceId;
        ReferenceNumber = Check.Length(referenceNumber, nameof(referenceNumber), ShopNotificationConsts.ReferenceNumberMaxLength);
        NavigationUrl = Check.Length(navigationUrl, nameof(navigationUrl), ShopNotificationConsts.NavigationUrlMaxLength);
        ActionLabel = Check.Length(actionLabel, nameof(actionLabel), ShopNotificationConsts.ActionLabelMaxLength);
        SourceDataJson = Check.Length(sourceDataJson, nameof(sourceDataJson), ShopNotificationConsts.SourceDataJsonMaxLength);
        TriggeredDate = triggeredDate;
        ExpiresDate = expiresDate;
    }

    internal void MarkAsRead(DateTime date)
    {
        if (Status is ShopNotificationStatus.Dismissed or ShopNotificationStatus.Resolved) return;
        Status = ShopNotificationStatus.Read;
        ReadDate = date;
    }

    internal void MarkAsUnread()
    {
        if (Status is ShopNotificationStatus.Dismissed or ShopNotificationStatus.Resolved) return;
        Status = ShopNotificationStatus.Unread;
        ReadDate = null;
    }

    internal void Dismiss(DateTime date)
    {
        Status = ShopNotificationStatus.Dismissed;
        DismissedDate = date;
    }

    internal void Resolve(DateTime date)
    {
        if (Status == ShopNotificationStatus.Resolved) return;
        Status = ShopNotificationStatus.Resolved;
        ResolvedDate = date;
    }
}
