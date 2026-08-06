using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.SupplierPayments;
using EHub.ShopManagement.Suppliers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace EHub.ShopManagement.Notifications.Generators;

/// <summary>
/// Supplier Payment Due and Supplier Overdue Balance. Goods Receipts have no due-date field, so a
/// due date is approximated as ReceiptDate + Supplier.PaymentTermsDays - the existing payment-terms
/// convention already on the Supplier entity - rather than inventing a new field.
///
/// Outstanding balance is computed from the same two tables
/// <see cref="ShopSupplierPaymentManager.GetPostedAllocatedAmountsAsync"/> reads (Posted
/// <see cref="ShopSupplierPayment"/> rows and their <see cref="ShopSupplierPaymentAllocation"/>
/// lines) rather than injecting that manager directly: <see cref="ShopSupplierPaymentManager"/>
/// itself calls into the notification system on Post (to resolve the overdue alert immediately),
/// so depending on it here would create a circular DI dependency
/// (ShopSupplierPaymentManager -&gt; IShopNotificationEvaluator -&gt; this generator -&gt;
/// ShopSupplierPaymentManager). The aggregation is identical, just read directly.
/// </summary>
public class SupplierDueNotificationGenerator : IShopNotificationGenerator, ITransientDependency
{
    private readonly IRepository<ShopGoodsReceipt, Guid> _goodsReceiptRepository;
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly IRepository<ShopSupplierPayment, Guid> _paymentRepository;
    private readonly IRepository<ShopSupplierPaymentAllocation, Guid> _allocationRepository;
    private readonly ShopNotificationManager _notificationManager;
    private readonly ShopNotificationSettingsProvider _settingsProvider;
    private readonly IClock _clock;

    public SupplierDueNotificationGenerator(
        IRepository<ShopGoodsReceipt, Guid> goodsReceiptRepository,
        IRepository<ShopSupplier, Guid> supplierRepository,
        IRepository<ShopSupplierPayment, Guid> paymentRepository,
        IRepository<ShopSupplierPaymentAllocation, Guid> allocationRepository,
        ShopNotificationManager notificationManager,
        ShopNotificationSettingsProvider settingsProvider,
        IClock clock)
    {
        _goodsReceiptRepository = goodsReceiptRepository;
        _supplierRepository = supplierRepository;
        _paymentRepository = paymentRepository;
        _allocationRepository = allocationRepository;
        _notificationManager = notificationManager;
        _settingsProvider = settingsProvider;
        _clock = clock;
    }

    public async Task GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsProvider.GetOrDefaultAsync(tenantId);
        if (!settings.NotificationsEnabled || !settings.SupplierDueNotificationsEnabled) return;

        var (dueActive, overdueActive) = await EvaluateAsync(tenantId, settings.SupplierDueReminderDays);

        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.SupplierPaymentDue, dueActive);
        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.SupplierOverdueBalance, overdueActive);
    }

    /// <summary>Narrow re-check for one supplier, used by the immediate hook after a supplier payment is posted.</summary>
    public async Task ResolveSupplierAsync(Guid tenantId, Guid supplierId)
    {
        var outstanding = await ComputeOutstandingAsync(tenantId, supplierId);
        if (outstanding <= 0)
        {
            await _notificationManager.ResolveBySourceKeyAsync(tenantId, DueKey(tenantId, supplierId));
            await _notificationManager.ResolveBySourceKeyAsync(tenantId, OverdueKey(tenantId, supplierId));
        }
    }

    private async Task<(HashSet<Guid> Due, HashSet<Guid> Overdue)> EvaluateAsync(Guid tenantId, int reminderDays)
    {
        var dueActive = new HashSet<Guid>();
        var overdueActive = new HashSet<Guid>();

        var receipts = await _goodsReceiptRepository.GetListAsync(x => x.TenantId == tenantId && x.Status == ShopGoodsReceiptStatus.Completed);
        if (receipts.Count == 0) return (dueActive, overdueActive);

        var supplierIds = receipts.Select(x => x.SupplierId).Distinct().ToList();
        var suppliers = await _supplierRepository.GetListAsync(x => supplierIds.Contains(x.Id));
        var supplierDict = suppliers.ToDictionary(x => x.Id);

        var receiptIds = receipts.Select(x => x.Id).ToList();
        var allocated = await GetAllocatedAmountsAsync(tenantId, receiptIds);
        var today = _clock.Now.Date;

        foreach (var group in receipts.GroupBy(x => x.SupplierId))
        {
            if (!supplierDict.TryGetValue(group.Key, out var supplier)) continue;

            var totalGrand = group.Sum(x => x.GrandTotal);
            var totalAllocated = group.Sum(x => allocated.GetValueOrDefault(x.Id));
            var outstanding = Math.Round(totalGrand - totalAllocated, 2);
            if (outstanding <= 0) continue;

            var latestReceiptDate = group.Max(x => x.ReceiptDate);
            var dueDate = latestReceiptDate.AddDays(supplier.PaymentTermsDays).Date;

            if (dueDate < today)
            {
                overdueActive.Add(group.Key);
                var overdueDays = (today - dueDate).Days;
                await _notificationManager.CreateOrUpdateAsync(
                    tenantId, ShopNotificationType.SupplierOverdueBalance, overdueDays > 30 ? ShopNotificationSeverity.Critical : ShopNotificationSeverity.Warning,
                    "Supplier Overdue Balance",
                    $"Supplier \"{supplier.Name}\" has an overdue balance of {outstanding:N2}.",
                    OverdueKey(tenantId, group.Key),
                    referenceType: "Supplier", referenceId: group.Key, referenceNumber: supplier.Code,
                    navigationUrl: ShopNotificationNavigation.SupplierLedger(group.Key), actionLabel: "ViewSupplierLedger");
            }
            else if ((dueDate - today).Days <= reminderDays)
            {
                dueActive.Add(group.Key);
                await _notificationManager.CreateOrUpdateAsync(
                    tenantId, ShopNotificationType.SupplierPaymentDue, ShopNotificationSeverity.Information,
                    "Supplier Payment Due",
                    $"Supplier \"{supplier.Name}\" has a payment of {outstanding:N2} due on {dueDate:yyyy-MM-dd}.",
                    DueKey(tenantId, group.Key),
                    referenceType: "Supplier", referenceId: group.Key, referenceNumber: supplier.Code,
                    navigationUrl: ShopNotificationNavigation.SupplierLedger(group.Key), actionLabel: "ViewSupplierLedger");
            }
        }

        return (dueActive, overdueActive);
    }

    private async Task<decimal> ComputeOutstandingAsync(Guid tenantId, Guid supplierId)
    {
        var receipts = await _goodsReceiptRepository.GetListAsync(x => x.TenantId == tenantId && x.SupplierId == supplierId && x.Status == ShopGoodsReceiptStatus.Completed);
        if (receipts.Count == 0) return 0;

        var receiptIds = receipts.Select(x => x.Id).ToList();
        var allocated = await GetAllocatedAmountsAsync(tenantId, receiptIds);
        return Math.Round(receipts.Sum(x => x.GrandTotal) - receipts.Sum(x => allocated.GetValueOrDefault(x.Id)), 2);
    }

    /// <summary>Sum of Posted payment allocations per Goods Receipt - same computation as ShopSupplierPaymentManager.GetPostedAllocatedAmountsAsync, read directly to avoid a circular dependency (see class remarks).</summary>
    private async Task<Dictionary<Guid, decimal>> GetAllocatedAmountsAsync(Guid tenantId, IReadOnlyList<Guid> goodsReceiptIds)
    {
        if (goodsReceiptIds.Count == 0) return new Dictionary<Guid, decimal>();

        var postedPayments = await _paymentRepository.GetListAsync(p => p.TenantId == tenantId && p.Status == ShopSupplierPaymentStatus.Posted);
        if (postedPayments.Count == 0) return new Dictionary<Guid, decimal>();

        var postedPaymentIds = postedPayments.Select(p => p.Id).ToHashSet();
        var allocations = await _allocationRepository.GetListAsync(a =>
            a.TenantId == tenantId && postedPaymentIds.Contains(a.SupplierPaymentId) && goodsReceiptIds.Contains(a.GoodsReceiptId));

        return allocations
            .GroupBy(a => a.GoodsReceiptId)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.AllocatedAmount));
    }

    private static string DueKey(Guid tenantId, Guid supplierId) => $"SupplierDue:{tenantId}:{supplierId}";
    private static string OverdueKey(Guid tenantId, Guid supplierId) => $"SupplierOverdue:{tenantId}:{supplierId}";
}
