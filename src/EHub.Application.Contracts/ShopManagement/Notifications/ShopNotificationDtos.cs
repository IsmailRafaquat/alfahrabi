using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Notifications;

public class ShopNotificationDto : EntityDto<Guid>
{
    public ShopNotificationType Type { get; set; }
    public ShopNotificationSeverity Severity { get; set; }
    public ShopNotificationStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? NavigationUrl { get; set; }
    public string? ActionLabel { get; set; }
    public DateTime TriggeredDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime? ExpiresDate { get; set; }
    public bool IsUnread { get; set; }
}

public class GetShopNotificationsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public ShopNotificationType? Type { get; set; }
    public ShopNotificationSeverity? Severity { get; set; }
    public ShopNotificationStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool UnreadOnly { get; set; }
    public string? ReferenceType { get; set; }
}

public class ShopNotificationSummaryDto
{
    public long TotalUnread { get; set; }
    public long InformationCount { get; set; }
    public long WarningCount { get; set; }
    public long CriticalCount { get; set; }
    public long TodayCount { get; set; }
    public long LowStockCount { get; set; }
    public long ExpiryCount { get; set; }
    public long PaymentDueCount { get; set; }
    public long DraftPendingCount { get; set; }
}

public class ShopNotificationGroupDto
{
    public string GroupKey { get; set; } = string.Empty;
    public string GroupLabel { get; set; } = string.Empty;
    public long Count { get; set; }
    public long UnreadCount { get; set; }
}

public class ShopNotificationSettingsDto
{
    public bool NotificationsEnabled { get; set; }
    public bool LowStockNotificationsEnabled { get; set; }
    public bool OutOfStockNotificationsEnabled { get; set; }
    public bool NearExpiryNotificationsEnabled { get; set; }
    public bool ExpiredBatchNotificationsEnabled { get; set; }
    public bool CustomerDueNotificationsEnabled { get; set; }
    public bool SupplierDueNotificationsEnabled { get; set; }
    public bool CashDifferenceNotificationsEnabled { get; set; }
    public bool BankLowBalanceNotificationsEnabled { get; set; }
    public bool PendingDraftNotificationsEnabled { get; set; }
    public bool ProfitLossWarningNotificationsEnabled { get; set; }

    public int NearExpiryDefaultDays { get; set; }
    public int CustomerDueReminderDays { get; set; }
    public int SupplierDueReminderDays { get; set; }
    public int DraftPendingHours { get; set; }
    public decimal BankLowBalanceThreshold { get; set; }
    public decimal ProfitLossWarningThreshold { get; set; }
    public int NotificationRetentionDays { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
}

public class UpdateShopNotificationSettingsDto
{
    public bool NotificationsEnabled { get; set; } = true;
    public bool LowStockNotificationsEnabled { get; set; } = true;
    public bool OutOfStockNotificationsEnabled { get; set; } = true;
    public bool NearExpiryNotificationsEnabled { get; set; } = true;
    public bool ExpiredBatchNotificationsEnabled { get; set; } = true;
    public bool CustomerDueNotificationsEnabled { get; set; } = true;
    public bool SupplierDueNotificationsEnabled { get; set; } = true;
    public bool CashDifferenceNotificationsEnabled { get; set; } = true;
    public bool BankLowBalanceNotificationsEnabled { get; set; } = true;
    public bool PendingDraftNotificationsEnabled { get; set; } = true;
    public bool ProfitLossWarningNotificationsEnabled { get; set; } = true;

    [Range(0, 3650)]
    public int NearExpiryDefaultDays { get; set; } = 30;
    [Range(0, 3650)]
    public int CustomerDueReminderDays { get; set; } = 3;
    [Range(0, 3650)]
    public int SupplierDueReminderDays { get; set; } = 3;
    [Range(0, 8760)]
    public int DraftPendingHours { get; set; } = 24;
    [Range(0, double.MaxValue)]
    public decimal BankLowBalanceThreshold { get; set; }
    [Range(0, 100)]
    public decimal ProfitLossWarningThreshold { get; set; }
    [Range(1, 3650)]
    public int NotificationRetentionDays { get; set; } = 90;
    public bool EmailNotificationsEnabled { get; set; }
}
