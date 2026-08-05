using System;
using System.Threading;
using System.Threading.Tasks;
using EHub.ShopManagement.ProfitLoss;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace EHub.ShopManagement.Notifications.Generators;

/// <summary>
/// Warns when the current month is showing a net loss, or a net profit margin below the configured
/// threshold. Uses <see cref="ShopProfitLossCalculator"/> directly (same Domain layer, no signed-in
/// user) so it evaluates the exact same numbers the Profit and Loss screen shows - this generator
/// itself is never gated by a user's ViewCost/ViewMargins permission (there is no user here); the
/// resulting notification is gated for readers via ShopNotifications.ViewProfitLossAlerts.
/// </summary>
public class ProfitLossNotificationGenerator : IShopNotificationGenerator, ITransientDependency
{
    private readonly ShopProfitLossCalculator _calculator;
    private readonly ShopNotificationManager _notificationManager;
    private readonly ShopNotificationSettingsProvider _settingsProvider;
    private readonly IClock _clock;

    public ProfitLossNotificationGenerator(
        ShopProfitLossCalculator calculator,
        ShopNotificationManager notificationManager,
        ShopNotificationSettingsProvider settingsProvider,
        IClock clock)
    {
        _calculator = calculator;
        _notificationManager = notificationManager;
        _settingsProvider = settingsProvider;
        _clock = clock;
    }

    public async Task GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsProvider.GetOrDefaultAsync(tenantId);
        if (!settings.NotificationsEnabled || !settings.ProfitLossWarningNotificationsEnabled) return;

        var today = _clock.Now.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEndExclusive = today.AddDays(1);

        var result = await _calculator.ComputeAggregateAsync(tenantId, monthStart, monthEndExclusive);
        var sourceKey = $"ProfitLossWarning:{tenantId}:{today.Year}:{today.Month}";

        var marginPercentage = result.NetSales > 0 ? Math.Round(result.NetProfit / result.NetSales * 100, 2) : (decimal?)null;
        var isWarning = result.NetProfit < 0 || (marginPercentage.HasValue && marginPercentage.Value < settings.ProfitLossWarningThreshold);

        if (!isWarning)
        {
            await _notificationManager.ResolveBySourceKeyAsync(tenantId, sourceKey);
            return;
        }

        var severity = result.NetProfit < 0 ? ShopNotificationSeverity.Critical : ShopNotificationSeverity.Warning;
        var message = result.NetProfit < 0
            ? $"Current month is showing a net loss of {Math.Abs(result.NetProfit):N2}."
            : $"Current month net profit margin ({marginPercentage:N2}%) is below the configured threshold ({settings.ProfitLossWarningThreshold:N2}%).";

        await _notificationManager.CreateOrUpdateAsync(
            tenantId, ShopNotificationType.ProfitLossWarning, severity,
            "Profit/Loss Warning", message, sourceKey,
            referenceType: "ProfitLoss", referenceId: null, referenceNumber: $"{today.Year}-{today.Month:00}",
            navigationUrl: ShopNotificationNavigation.ProfitLoss(), actionLabel: "ViewProfitLoss");
    }
}
