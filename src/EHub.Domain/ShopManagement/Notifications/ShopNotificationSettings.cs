using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Notifications;

/// <summary>One row per tenant - notification rule configuration, mirrors the ShopSetting pattern.</summary>
public class ShopNotificationSettings : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public bool NotificationsEnabled { get; protected set; } = true;
    public bool LowStockNotificationsEnabled { get; protected set; } = true;
    public bool OutOfStockNotificationsEnabled { get; protected set; } = true;
    public bool NearExpiryNotificationsEnabled { get; protected set; } = true;
    public bool ExpiredBatchNotificationsEnabled { get; protected set; } = true;
    public bool CustomerDueNotificationsEnabled { get; protected set; } = true;
    public bool SupplierDueNotificationsEnabled { get; protected set; } = true;
    public bool CashDifferenceNotificationsEnabled { get; protected set; } = true;
    public bool BankLowBalanceNotificationsEnabled { get; protected set; } = true;
    public bool PendingDraftNotificationsEnabled { get; protected set; } = true;
    public bool ProfitLossWarningNotificationsEnabled { get; protected set; } = true;

    public int NearExpiryDefaultDays { get; protected set; } = 30;
    public int CustomerDueReminderDays { get; protected set; } = 3;
    public int SupplierDueReminderDays { get; protected set; } = 3;
    public int DraftPendingHours { get; protected set; } = 24;
    public decimal BankLowBalanceThreshold { get; protected set; }
    /// <summary>Net profit margin percentage - warn when this month's margin falls below it (or is a loss).</summary>
    public decimal ProfitLossWarningThreshold { get; protected set; }
    public int NotificationRetentionDays { get; protected set; } = 90;

    /// <summary>Only whether email notifications are on for this tenant - never SMTP/infrastructure credentials.</summary>
    public bool EmailNotificationsEnabled { get; protected set; }

    public DateTime? LastExpiryCheckDate { get; protected set; }
    public DateTime? LastProfitLossCheckDate { get; protected set; }

    protected ShopNotificationSettings() { }

    public ShopNotificationSettings(Guid id, Guid tenantId) : base(id)
    {
        TenantId = tenantId;
    }

    public void Update(
        bool notificationsEnabled, bool lowStockEnabled, bool outOfStockEnabled, bool nearExpiryEnabled,
        bool expiredBatchEnabled, bool customerDueEnabled, bool supplierDueEnabled, bool cashDifferenceEnabled,
        bool bankLowBalanceEnabled, bool pendingDraftEnabled, bool profitLossWarningEnabled,
        int nearExpiryDefaultDays, int customerDueReminderDays, int supplierDueReminderDays, int draftPendingHours,
        decimal bankLowBalanceThreshold, decimal profitLossWarningThreshold, int notificationRetentionDays,
        bool emailNotificationsEnabled)
    {
        if (nearExpiryDefaultDays < 0 || customerDueReminderDays < 0 || supplierDueReminderDays < 0 ||
            draftPendingHours < 0 || notificationRetentionDays < 0 || bankLowBalanceThreshold < 0 ||
            profitLossWarningThreshold < 0)
        {
            throw new BusinessException("ShopManagement:InvalidNotificationSettings");
        }

        NotificationsEnabled = notificationsEnabled;
        LowStockNotificationsEnabled = lowStockEnabled;
        OutOfStockNotificationsEnabled = outOfStockEnabled;
        NearExpiryNotificationsEnabled = nearExpiryEnabled;
        ExpiredBatchNotificationsEnabled = expiredBatchEnabled;
        CustomerDueNotificationsEnabled = customerDueEnabled;
        SupplierDueNotificationsEnabled = supplierDueEnabled;
        CashDifferenceNotificationsEnabled = cashDifferenceEnabled;
        BankLowBalanceNotificationsEnabled = bankLowBalanceEnabled;
        PendingDraftNotificationsEnabled = pendingDraftEnabled;
        ProfitLossWarningNotificationsEnabled = profitLossWarningEnabled;
        NearExpiryDefaultDays = nearExpiryDefaultDays;
        CustomerDueReminderDays = customerDueReminderDays;
        SupplierDueReminderDays = supplierDueReminderDays;
        DraftPendingHours = draftPendingHours;
        BankLowBalanceThreshold = bankLowBalanceThreshold;
        ProfitLossWarningThreshold = profitLossWarningThreshold;
        NotificationRetentionDays = notificationRetentionDays;
        EmailNotificationsEnabled = emailNotificationsEnabled;
    }

    internal void MarkExpiryChecked(DateTime date) => LastExpiryCheckDate = date;
    internal void MarkProfitLossChecked(DateTime date) => LastProfitLossCheckDate = date;
}
