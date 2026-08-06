using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.Sales;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace EHub.ShopManagement.Notifications.Generators;

/// <summary>
/// Customer Payment Due (Sale.DueDate within the reminder window) and Customer Overdue Balance
/// (Sale.DueDate already passed) - uses the existing Sale.DueDate field directly, per credit sale,
/// rather than inventing a separate due-date convention.
/// </summary>
public class CustomerDueNotificationGenerator : IShopNotificationGenerator, ITransientDependency
{
    private readonly IRepository<ShopSale, Guid> _saleRepository;
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly ShopNotificationManager _notificationManager;
    private readonly ShopNotificationSettingsProvider _settingsProvider;
    private readonly IClock _clock;

    public CustomerDueNotificationGenerator(
        IRepository<ShopSale, Guid> saleRepository,
        IRepository<ShopCustomer, Guid> customerRepository,
        ShopNotificationManager notificationManager,
        ShopNotificationSettingsProvider settingsProvider,
        IClock clock)
    {
        _saleRepository = saleRepository;
        _customerRepository = customerRepository;
        _notificationManager = notificationManager;
        _settingsProvider = settingsProvider;
        _clock = clock;
    }

    public async Task GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsProvider.GetOrDefaultAsync(tenantId);
        if (!settings.NotificationsEnabled || !settings.CustomerDueNotificationsEnabled) return;

        var (dueActive, overdueActive) = await EvaluateAsync(tenantId, settings.CustomerDueReminderDays);

        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.CustomerPaymentDue, dueActive);
        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.CustomerOverdueBalance, overdueActive);
    }

    /// <summary>Narrow re-check for one customer, used by the immediate hook after a customer payment is posted.</summary>
    public async Task ResolveCustomerAsync(Guid tenantId, Guid customerId)
    {
        var sales = await _saleRepository.GetListAsync(x =>
            x.TenantId == tenantId && x.CustomerId == customerId && x.Status == ShopSaleStatus.Completed && x.PendingAmount > 0 && x.DueDate.HasValue);

        if (sales.Count == 0)
        {
            await _notificationManager.ResolveBySourceKeyAsync(tenantId, DueKey(tenantId, customerId));
            await _notificationManager.ResolveBySourceKeyAsync(tenantId, OverdueKey(tenantId, customerId));
        }
    }

    private async Task<(HashSet<Guid> Due, HashSet<Guid> Overdue)> EvaluateAsync(Guid tenantId, int reminderDays)
    {
        var dueActive = new HashSet<Guid>();
        var overdueActive = new HashSet<Guid>();

        var sales = await _saleRepository.GetListAsync(x =>
            x.TenantId == tenantId && x.Status == ShopSaleStatus.Completed && x.PendingAmount > 0 && x.DueDate.HasValue);
        if (sales.Count == 0) return (dueActive, overdueActive);

        var today = _clock.Now.Date;
        var customerIds = sales.Select(x => x.CustomerId).Distinct().ToList();
        var customers = await _customerRepository.GetListAsync(x => customerIds.Contains(x.Id));
        var customerDict = customers.ToDictionary(x => x.Id);

        foreach (var group in sales.GroupBy(x => x.CustomerId))
        {
            if (!customerDict.TryGetValue(group.Key, out var customer)) continue;

            var outstanding = Math.Round(group.Sum(x => x.PendingAmount), 2);
            var earliestDueDate = group.Min(x => x.DueDate!.Value.Date);

            if (earliestDueDate < today)
            {
                overdueActive.Add(group.Key);
                var overdueDays = (today - earliestDueDate).Days;
                await _notificationManager.CreateOrUpdateAsync(
                    tenantId, ShopNotificationType.CustomerOverdueBalance, SeverityForOverdue(overdueDays, outstanding),
                    "Customer Overdue Balance",
                    $"Customer \"{customer.Name}\" has an overdue balance of {outstanding:N2}.",
                    OverdueKey(tenantId, group.Key),
                    referenceType: "Customer", referenceId: group.Key, referenceNumber: null,
                    navigationUrl: ShopNotificationNavigation.CustomerLedger(group.Key), actionLabel: "ViewCustomerLedger");
            }
            else if ((earliestDueDate - today).Days <= reminderDays)
            {
                dueActive.Add(group.Key);
                await _notificationManager.CreateOrUpdateAsync(
                    tenantId, ShopNotificationType.CustomerPaymentDue, ShopNotificationSeverity.Information,
                    "Customer Payment Due",
                    $"Customer \"{customer.Name}\" has a payment of {outstanding:N2} due on {earliestDueDate:yyyy-MM-dd}.",
                    DueKey(tenantId, group.Key),
                    referenceType: "Customer", referenceId: group.Key, referenceNumber: null,
                    navigationUrl: ShopNotificationNavigation.CustomerLedger(group.Key), actionLabel: "ViewCustomerLedger");
            }
        }

        return (dueActive, overdueActive);
    }

    private static ShopNotificationSeverity SeverityForOverdue(int overdueDays, decimal outstanding) =>
        overdueDays > 30 || outstanding > 100000 ? ShopNotificationSeverity.Critical : ShopNotificationSeverity.Warning;

    private static string DueKey(Guid tenantId, Guid customerId) => $"CustomerDue:{tenantId}:{customerId}";
    private static string OverdueKey(Guid tenantId, Guid customerId) => $"CustomerOverdue:{tenantId}:{customerId}";
}
