using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace EHub.ShopManagement.Notifications;

/// <summary>
/// Read/write surface over notifications for the current user: lists only what they're allowed to
/// see (assigned to them, or tenant-wide), computing effective read/unread state per user for
/// tenant-wide notifications via <see cref="ShopNotificationUserState"/>. Restricted types
/// (financial, Profit/Loss) are filtered out server-side for callers lacking the matching
/// permission - never left to the frontend to hide.
/// </summary>
[Authorize(EHubPermissions.ShopNotifications.View)]
public class ShopNotificationAppService : ApplicationService, IShopNotificationAppService
{
    private static readonly HashSet<ShopNotificationType> FinancialTypes = new()
    {
        ShopNotificationType.CustomerPaymentDue, ShopNotificationType.SupplierPaymentDue,
        ShopNotificationType.CustomerOverdueBalance, ShopNotificationType.SupplierOverdueBalance,
        ShopNotificationType.CashClosingDifference, ShopNotificationType.BankLowBalance,
    };

    private readonly IRepository<ShopNotification, Guid> _notificationRepository;
    private readonly IRepository<ShopNotificationUserState, Guid> _userStateRepository;
    private readonly IRepository<ShopNotificationSettings, Guid> _settingsRepository;
    private readonly ShopNotificationManager _notificationManager;
    private readonly IEnumerable<IShopNotificationGenerator> _generators;

    public ShopNotificationAppService(
        IRepository<ShopNotification, Guid> notificationRepository,
        IRepository<ShopNotificationUserState, Guid> userStateRepository,
        IRepository<ShopNotificationSettings, Guid> settingsRepository,
        ShopNotificationManager notificationManager,
        IEnumerable<IShopNotificationGenerator> generators)
    {
        _notificationRepository = notificationRepository;
        _userStateRepository = userStateRepository;
        _settingsRepository = settingsRepository;
        _notificationManager = notificationManager;
        _generators = generators;
    }

    public async Task<PagedResultDto<ShopNotificationDto>> GetListAsync(GetShopNotificationsInput input)
    {
        var tenantId = RequireTenant();
        var userId = CurrentUser.GetId();

        var visible = await LoadVisibleAsync(tenantId, userId, input);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            visible = visible.Where(x =>
                x.Notification.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Notification.Message.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (x.Notification.ReferenceNumber != null && x.Notification.ReferenceNumber.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        if (input.Status.HasValue) visible = visible.Where(x => x.EffectiveStatus == input.Status.Value).ToList();
        if (input.UnreadOnly) visible = visible.Where(x => x.EffectiveStatus == ShopNotificationStatus.Unread).ToList();

        var ordered = visible
            .OrderBy(x => x.EffectiveStatus == ShopNotificationStatus.Unread ? 0 : 1)
            .ThenByDescending(x => x.Notification.Severity)
            .ThenByDescending(x => x.Notification.TriggeredDate)
            .ToList();

        var totalCount = ordered.Count;
        var page = ordered.Skip(input.SkipCount).Take(input.MaxResultCount == 0 ? 20 : input.MaxResultCount).ToList();

        return new PagedResultDto<ShopNotificationDto>(totalCount, page.Select(ToDto).ToList());
    }

    public async Task<ShopNotificationDto> GetAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var userId = CurrentUser.GetId();

        var notification = await _notificationRepository.FindAsync(id);
        if (notification == null || notification.TenantId != tenantId) throw new BusinessException("ShopManagement:NotificationNotFound");
        if (notification.UserId.HasValue && notification.UserId.Value != userId) throw new BusinessException("ShopManagement:NotificationNotFound");
        await EnsureVisibleTypeAsync(notification.Type);

        var effectiveStatus = await ComputeEffectiveStatusAsync(notification, userId);
        return ToDto((notification, effectiveStatus));
    }

    public async Task<ShopNotificationSummaryDto> GetSummaryAsync()
    {
        var tenantId = RequireTenant();
        var userId = CurrentUser.GetId();
        var today = Clock.Now.Date;

        var visible = await LoadVisibleAsync(tenantId, userId, new GetShopNotificationsInput { MaxResultCount = int.MaxValue });

        return new ShopNotificationSummaryDto
        {
            TotalUnread = visible.Count(x => x.EffectiveStatus == ShopNotificationStatus.Unread),
            InformationCount = visible.Count(x => x.EffectiveStatus == ShopNotificationStatus.Unread && x.Notification.Severity == ShopNotificationSeverity.Information),
            WarningCount = visible.Count(x => x.EffectiveStatus == ShopNotificationStatus.Unread && x.Notification.Severity == ShopNotificationSeverity.Warning),
            CriticalCount = visible.Count(x => x.EffectiveStatus == ShopNotificationStatus.Unread && x.Notification.Severity == ShopNotificationSeverity.Critical),
            TodayCount = visible.Count(x => x.Notification.TriggeredDate.Date == today),
            LowStockCount = visible.Count(x => x.EffectiveStatus == ShopNotificationStatus.Unread && (x.Notification.Type == ShopNotificationType.LowStock || x.Notification.Type == ShopNotificationType.OutOfStock)),
            ExpiryCount = visible.Count(x => x.EffectiveStatus == ShopNotificationStatus.Unread && (x.Notification.Type == ShopNotificationType.NearExpiry || x.Notification.Type == ShopNotificationType.ExpiredBatch)),
            PaymentDueCount = visible.Count(x => x.EffectiveStatus == ShopNotificationStatus.Unread && FinancialTypes.Contains(x.Notification.Type)),
            DraftPendingCount = visible.Count(x => x.EffectiveStatus == ShopNotificationStatus.Unread &&
                (x.Notification.Type == ShopNotificationType.DraftSalePending || x.Notification.Type == ShopNotificationType.DraftPurchasePending ||
                 x.Notification.Type == ShopNotificationType.UnpostedExpense || x.Notification.Type == ShopNotificationType.StockCountPending ||
                 x.Notification.Type == ShopNotificationType.StockAdjustmentPending)),
        };
    }

    [Authorize(EHubPermissions.ShopNotifications.MarkRead)]
    public async Task MarkAsReadAsync(Guid id)
    {
        var tenantId = RequireTenant();
        await _notificationManager.MarkAsReadAsync(tenantId, id, CurrentUser.GetId());
    }

    [Authorize(EHubPermissions.ShopNotifications.MarkRead)]
    public async Task MarkAsUnreadAsync(Guid id)
    {
        var tenantId = RequireTenant();
        await _notificationManager.MarkAsUnreadAsync(tenantId, id, CurrentUser.GetId());
    }

    [Authorize(EHubPermissions.ShopNotifications.MarkRead)]
    public async Task MarkAllAsReadAsync()
    {
        var tenantId = RequireTenant();
        var userId = CurrentUser.GetId();
        var visible = await LoadVisibleAsync(tenantId, userId, new GetShopNotificationsInput { MaxResultCount = int.MaxValue, UnreadOnly = true });

        foreach (var item in visible)
        {
            await _notificationManager.MarkAsReadAsync(tenantId, item.Notification.Id, userId);
        }
    }

    [Authorize(EHubPermissions.ShopNotifications.Dismiss)]
    public async Task DismissAsync(Guid id)
    {
        var tenantId = RequireTenant();
        await _notificationManager.DismissAsync(tenantId, id, CurrentUser.GetId());
    }

    [Authorize(EHubPermissions.ShopNotifications.Dismiss)]
    public async Task DismissAllAsync()
    {
        var tenantId = RequireTenant();
        var userId = CurrentUser.GetId();
        var visible = await LoadVisibleAsync(tenantId, userId, new GetShopNotificationsInput { MaxResultCount = int.MaxValue });

        foreach (var item in visible.Where(x => x.EffectiveStatus != ShopNotificationStatus.Dismissed && x.EffectiveStatus != ShopNotificationStatus.Resolved))
        {
            await _notificationManager.DismissAsync(tenantId, item.Notification.Id, userId);
        }
    }

    [Authorize(EHubPermissions.ShopNotifications.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var tenantId = RequireTenant();
        await _notificationManager.DeleteAsync(tenantId, id, CurrentUser.GetId());
    }

    public async Task<ShopNotificationSettingsDto> GetSettingsAsync()
    {
        var tenantId = RequireTenant();
        var settings = await _settingsRepository.FirstOrDefaultAsync(x => x.TenantId == tenantId);
        settings ??= new ShopNotificationSettings(Guid.Empty, tenantId);

        return new ShopNotificationSettingsDto
        {
            NotificationsEnabled = settings.NotificationsEnabled,
            LowStockNotificationsEnabled = settings.LowStockNotificationsEnabled,
            OutOfStockNotificationsEnabled = settings.OutOfStockNotificationsEnabled,
            NearExpiryNotificationsEnabled = settings.NearExpiryNotificationsEnabled,
            ExpiredBatchNotificationsEnabled = settings.ExpiredBatchNotificationsEnabled,
            CustomerDueNotificationsEnabled = settings.CustomerDueNotificationsEnabled,
            SupplierDueNotificationsEnabled = settings.SupplierDueNotificationsEnabled,
            CashDifferenceNotificationsEnabled = settings.CashDifferenceNotificationsEnabled,
            BankLowBalanceNotificationsEnabled = settings.BankLowBalanceNotificationsEnabled,
            PendingDraftNotificationsEnabled = settings.PendingDraftNotificationsEnabled,
            ProfitLossWarningNotificationsEnabled = settings.ProfitLossWarningNotificationsEnabled,
            NearExpiryDefaultDays = settings.NearExpiryDefaultDays,
            CustomerDueReminderDays = settings.CustomerDueReminderDays,
            SupplierDueReminderDays = settings.SupplierDueReminderDays,
            DraftPendingHours = settings.DraftPendingHours,
            BankLowBalanceThreshold = settings.BankLowBalanceThreshold,
            ProfitLossWarningThreshold = settings.ProfitLossWarningThreshold,
            NotificationRetentionDays = settings.NotificationRetentionDays,
            EmailNotificationsEnabled = settings.EmailNotificationsEnabled,
        };
    }

    [Authorize(EHubPermissions.ShopNotifications.ManageSettings)]
    public async Task UpdateSettingsAsync(UpdateShopNotificationSettingsDto input)
    {
        var tenantId = RequireTenant();
        var settings = await _settingsRepository.FirstOrDefaultAsync(x => x.TenantId == tenantId);

        if (settings == null)
        {
            settings = new ShopNotificationSettings(GuidGenerator.Create(), tenantId);
            await _settingsRepository.InsertAsync(settings, autoSave: true);
        }

        settings.Update(
            input.NotificationsEnabled, input.LowStockNotificationsEnabled, input.OutOfStockNotificationsEnabled,
            input.NearExpiryNotificationsEnabled, input.ExpiredBatchNotificationsEnabled, input.CustomerDueNotificationsEnabled,
            input.SupplierDueNotificationsEnabled, input.CashDifferenceNotificationsEnabled, input.BankLowBalanceNotificationsEnabled,
            input.PendingDraftNotificationsEnabled, input.ProfitLossWarningNotificationsEnabled,
            input.NearExpiryDefaultDays, input.CustomerDueReminderDays, input.SupplierDueReminderDays, input.DraftPendingHours,
            input.BankLowBalanceThreshold, input.ProfitLossWarningThreshold, input.NotificationRetentionDays,
            input.EmailNotificationsEnabled);

        await _settingsRepository.UpdateAsync(settings);
    }

    [Authorize(EHubPermissions.ShopNotifications.GenerateNow)]
    public async Task GenerateNowAsync()
    {
        var tenantId = RequireTenant();
        foreach (var generator in _generators)
        {
            await generator.GenerateAsync(tenantId);
        }
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:NotificationTenantRequired");

    private async Task EnsureVisibleTypeAsync(ShopNotificationType type)
    {
        if (type == ShopNotificationType.ProfitLossWarning && !await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopNotifications.ViewProfitLossAlerts))
            throw new BusinessException("ShopManagement:NotificationNotFound");
        if (FinancialTypes.Contains(type) && !await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopNotifications.ViewFinancialAlerts))
            throw new BusinessException("ShopManagement:NotificationNotFound");
    }

    private async Task<List<(ShopNotification Notification, ShopNotificationStatus EffectiveStatus)>> LoadVisibleAsync(Guid tenantId, Guid userId, GetShopNotificationsInput input)
    {
        var canViewFinancialAlerts = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopNotifications.ViewFinancialAlerts);
        var canViewProfitLossAlerts = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopNotifications.ViewProfitLossAlerts);

        var query = (await _notificationRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && (x.UserId == null || x.UserId == userId));

        if (input.Type.HasValue) query = query.Where(x => x.Type == input.Type.Value);
        if (input.Severity.HasValue) query = query.Where(x => x.Severity == input.Severity.Value);
        if (input.DateFrom.HasValue) query = query.Where(x => x.TriggeredDate >= input.DateFrom.Value);
        if (input.DateTo.HasValue) query = query.Where(x => x.TriggeredDate < input.DateTo.Value.AddDays(1));
        if (!string.IsNullOrWhiteSpace(input.ReferenceType)) query = query.Where(x => x.ReferenceType == input.ReferenceType);
        if (!canViewFinancialAlerts) query = query.Where(x => !FinancialTypes.Contains(x.Type));
        if (!canViewProfitLossAlerts) query = query.Where(x => x.Type != ShopNotificationType.ProfitLossWarning);

        var notifications = await AsyncExecuter.ToListAsync(query);
        if (notifications.Count == 0) return new List<(ShopNotification, ShopNotificationStatus)>();

        var tenantWideIds = notifications.Where(x => !x.UserId.HasValue).Select(x => x.Id).ToList();
        Dictionary<Guid, ShopNotificationStatus> userStateByNotification = new();
        if (tenantWideIds.Count > 0)
        {
            var stateQuery = (await _userStateRepository.GetQueryableAsync()).AsNoTracking();
            var states = await AsyncExecuter.ToListAsync(
                stateQuery.Where(s => s.TenantId == tenantId && s.UserId == userId && tenantWideIds.Contains(s.NotificationId)));
            userStateByNotification = states.ToDictionary(s => s.NotificationId, s => s.Status);
        }

        return notifications
            .Select(n => (n, EffectiveStatus: n.UserId.HasValue ? n.Status : userStateByNotification.GetValueOrDefault(n.Id, ShopNotificationStatus.Unread)))
            .ToList();
    }

    private async Task<ShopNotificationStatus> ComputeEffectiveStatusAsync(ShopNotification notification, Guid userId)
    {
        if (notification.UserId.HasValue) return notification.Status;

        var state = await _userStateRepository.FirstOrDefaultAsync(
            x => x.TenantId == notification.TenantId && x.NotificationId == notification.Id && x.UserId == userId);
        return state?.Status ?? ShopNotificationStatus.Unread;
    }

    private static ShopNotificationDto ToDto((ShopNotification Notification, ShopNotificationStatus EffectiveStatus) item)
    {
        var n = item.Notification;
        return new ShopNotificationDto
        {
            Id = n.Id,
            Type = n.Type,
            Severity = n.Severity,
            Status = item.EffectiveStatus,
            Title = n.Title,
            Message = n.Message,
            ReferenceType = n.ReferenceType,
            ReferenceId = n.ReferenceId,
            ReferenceNumber = n.ReferenceNumber,
            NavigationUrl = n.NavigationUrl,
            ActionLabel = n.ActionLabel,
            TriggeredDate = n.TriggeredDate,
            ReadDate = n.UserId.HasValue ? n.ReadDate : null,
            ExpiresDate = n.ExpiresDate,
            IsUnread = item.EffectiveStatus == ShopNotificationStatus.Unread,
        };
    }
}
