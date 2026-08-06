using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.ShopManagement.BankAccounts;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.Notifications.Generators;

/// <summary>
/// Active Bank Account balance below the configured threshold. ShopBankAccount has no per-account
/// threshold field today, so <see cref="ShopNotificationSettings.BankLowBalanceThreshold"/> (a
/// single tenant-wide value) is used for every account - documented limitation, not an invented
/// per-account field.
/// </summary>
public class BankBalanceNotificationGenerator : IShopNotificationGenerator, ITransientDependency
{
    private readonly IRepository<ShopBankAccount, Guid> _bankAccountRepository;
    private readonly ShopNotificationManager _notificationManager;
    private readonly ShopNotificationSettingsProvider _settingsProvider;

    public BankBalanceNotificationGenerator(
        IRepository<ShopBankAccount, Guid> bankAccountRepository,
        ShopNotificationManager notificationManager,
        ShopNotificationSettingsProvider settingsProvider)
    {
        _bankAccountRepository = bankAccountRepository;
        _notificationManager = notificationManager;
        _settingsProvider = settingsProvider;
    }

    public async Task GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsProvider.GetOrDefaultAsync(tenantId);
        if (!settings.NotificationsEnabled || !settings.BankLowBalanceNotificationsEnabled) return;
        if (settings.BankLowBalanceThreshold <= 0) return;

        var accounts = await _bankAccountRepository.GetListAsync(x => x.TenantId == tenantId && x.IsActive);
        var active = new HashSet<Guid>();

        foreach (var account in accounts)
        {
            if (account.CurrentBalance >= settings.BankLowBalanceThreshold) continue;

            active.Add(account.Id);
            await _notificationManager.CreateOrUpdateAsync(
                tenantId, ShopNotificationType.BankLowBalance, ShopNotificationSeverity.Warning,
                "Bank Low Balance",
                $"Bank account \"{account.AccountName}\" balance ({account.CurrentBalance:N2}) is below the configured threshold ({settings.BankLowBalanceThreshold:N2}).",
                $"BankLowBalance:{tenantId}:{account.Id}",
                referenceType: "BankAccount", referenceId: account.Id, referenceNumber: account.Code,
                navigationUrl: ShopNotificationNavigation.BankAccount(account.Id), actionLabel: "ViewBankAccount");
        }

        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.BankLowBalance, active);
    }
}
