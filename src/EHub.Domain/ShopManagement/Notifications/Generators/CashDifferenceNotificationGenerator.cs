using System;
using System.Threading;
using System.Threading.Tasks;
using EHub.ShopManagement.CashRegisters;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.Notifications.Generators;

/// <summary>
/// A closed Cash Closing whose ActualClosingCash differs from ExpectedClosingCash. Each closing is
/// a one-shot historical event (not an ongoing condition), so once notified it is never resolved or
/// recreated by the sweep - see <see cref="ShopNotificationManager.ExistsBySourceKeyAsync"/>.
/// </summary>
public class CashDifferenceNotificationGenerator : IShopNotificationGenerator, ITransientDependency
{
    private readonly IRepository<ShopCashClosing, Guid> _cashClosingRepository;
    private readonly ShopNotificationManager _notificationManager;
    private readonly ShopNotificationSettingsProvider _settingsProvider;

    public CashDifferenceNotificationGenerator(
        IRepository<ShopCashClosing, Guid> cashClosingRepository,
        ShopNotificationManager notificationManager,
        ShopNotificationSettingsProvider settingsProvider)
    {
        _cashClosingRepository = cashClosingRepository;
        _notificationManager = notificationManager;
        _settingsProvider = settingsProvider;
    }

    public async Task GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsProvider.GetOrDefaultAsync(tenantId);
        if (!settings.NotificationsEnabled || !settings.CashDifferenceNotificationsEnabled) return;

        var closings = await _cashClosingRepository.GetListAsync(x =>
            x.TenantId == tenantId && x.Status == ShopCashClosingStatus.Closed &&
            x.ActualClosingCash.HasValue && x.DifferenceAmount.HasValue && x.DifferenceAmount != 0);

        foreach (var closing in closings)
        {
            await EvaluateAsync(tenantId, closing);
        }
    }

    /// <summary>Called immediately after a cash closing is posted.</summary>
    public async Task EvaluateClosingAsync(Guid tenantId, ShopCashClosing closing)
    {
        var settings = await _settingsProvider.GetOrDefaultAsync(tenantId);
        if (!settings.NotificationsEnabled || !settings.CashDifferenceNotificationsEnabled) return;
        if (!closing.DifferenceAmount.HasValue || closing.DifferenceAmount == 0) return;

        await EvaluateAsync(tenantId, closing);
    }

    private async Task EvaluateAsync(Guid tenantId, ShopCashClosing closing)
    {
        var sourceKey = $"CashDifference:{tenantId}:{closing.Id}";
        if (await _notificationManager.ExistsBySourceKeyAsync(tenantId, sourceKey)) return;

        var difference = closing.DifferenceAmount!.Value;
        var severity = Math.Abs(difference) >= closing.ExpectedClosingCash * 0.1m || Math.Abs(difference) >= 1000
            ? ShopNotificationSeverity.Critical
            : ShopNotificationSeverity.Warning;

        await _notificationManager.CreateOrUpdateAsync(
            tenantId, ShopNotificationType.CashClosingDifference, severity,
            "Cash Closing Difference",
            $"Cash closing for {closing.BusinessDate:yyyy-MM-dd} has a difference of {difference:N2}.",
            sourceKey,
            referenceType: "CashClosing", referenceId: closing.Id, referenceNumber: null,
            navigationUrl: ShopNotificationNavigation.CashClosing(closing.Id), actionLabel: "ViewCashClosing");
    }
}
