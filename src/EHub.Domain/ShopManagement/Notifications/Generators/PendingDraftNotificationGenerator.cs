using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.ShopManagement.Expenses;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.StockAdjustments;
using EHub.ShopManagement.StockCounts;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace EHub.ShopManagement.Notifications.Generators;

/// <summary>
/// Draft Sale, Draft Purchase Order, unposted Expense, pending Stock Count, and pending Stock
/// Adjustment records older than the configured threshold. Each document type resolves itself the
/// next time this sweeps and finds the record no longer in its "pending" status (posted, completed,
/// or cancelled) - no separate hook needed per document type.
/// </summary>
public class PendingDraftNotificationGenerator : IShopNotificationGenerator, ITransientDependency
{
    private readonly IRepository<ShopSale, Guid> _saleRepository;
    private readonly IRepository<ShopPurchaseOrder, Guid> _purchaseOrderRepository;
    private readonly IRepository<ShopExpense, Guid> _expenseRepository;
    private readonly IRepository<ShopStockCount, Guid> _stockCountRepository;
    private readonly IRepository<ShopStockAdjustment, Guid> _stockAdjustmentRepository;
    private readonly ShopNotificationManager _notificationManager;
    private readonly ShopNotificationSettingsProvider _settingsProvider;
    private readonly IClock _clock;

    public PendingDraftNotificationGenerator(
        IRepository<ShopSale, Guid> saleRepository,
        IRepository<ShopPurchaseOrder, Guid> purchaseOrderRepository,
        IRepository<ShopExpense, Guid> expenseRepository,
        IRepository<ShopStockCount, Guid> stockCountRepository,
        IRepository<ShopStockAdjustment, Guid> stockAdjustmentRepository,
        ShopNotificationManager notificationManager,
        ShopNotificationSettingsProvider settingsProvider,
        IClock clock)
    {
        _saleRepository = saleRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _expenseRepository = expenseRepository;
        _stockCountRepository = stockCountRepository;
        _stockAdjustmentRepository = stockAdjustmentRepository;
        _notificationManager = notificationManager;
        _settingsProvider = settingsProvider;
        _clock = clock;
    }

    public async Task GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsProvider.GetOrDefaultAsync(tenantId);
        if (!settings.NotificationsEnabled || !settings.PendingDraftNotificationsEnabled) return;

        var cutoff = _clock.Now.AddHours(-settings.DraftPendingHours);

        var salesActive = new HashSet<Guid>();
        var sales = await _saleRepository.GetListAsync(x => x.TenantId == tenantId && x.Status == ShopSaleStatus.Draft && x.CreationTime < cutoff);
        foreach (var sale in sales)
        {
            salesActive.Add(sale.Id);
            await _notificationManager.CreateOrUpdateAsync(
                tenantId, ShopNotificationType.DraftSalePending, ShopNotificationSeverity.Information,
                "Draft Sale Pending",
                $"Sale \"{sale.SaleNumber}\" has been in Draft status for over {settings.DraftPendingHours} hours.",
                $"DraftSalePending:{tenantId}:{sale.Id}",
                referenceType: "Sale", referenceId: sale.Id, referenceNumber: sale.SaleNumber,
                navigationUrl: ShopNotificationNavigation.Sale(sale.Id), actionLabel: "ViewSale");
        }
        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.DraftSalePending, salesActive);

        var purchasesActive = new HashSet<Guid>();
        var purchaseOrders = await _purchaseOrderRepository.GetListAsync(x =>
            x.TenantId == tenantId && (x.Status == ShopPurchaseOrderStatus.Draft || x.Status == ShopPurchaseOrderStatus.PendingApproval) && x.CreationTime < cutoff);
        foreach (var po in purchaseOrders)
        {
            purchasesActive.Add(po.Id);
            await _notificationManager.CreateOrUpdateAsync(
                tenantId, ShopNotificationType.DraftPurchasePending, ShopNotificationSeverity.Information,
                "Draft Purchase Pending",
                $"Purchase Order \"{po.PurchaseOrderNumber}\" has been pending for over {settings.DraftPendingHours} hours.",
                $"DraftPurchasePending:{tenantId}:{po.Id}",
                referenceType: "PurchaseOrder", referenceId: po.Id, referenceNumber: po.PurchaseOrderNumber,
                navigationUrl: ShopNotificationNavigation.PurchaseOrder(po.Id), actionLabel: "ViewPurchaseOrder");
        }
        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.DraftPurchasePending, purchasesActive);

        var expensesActive = new HashSet<Guid>();
        var expenses = await _expenseRepository.GetListAsync(x => x.TenantId == tenantId && x.Status == ShopExpenseStatus.Draft && x.CreationTime < cutoff);
        foreach (var expense in expenses)
        {
            expensesActive.Add(expense.Id);
            await _notificationManager.CreateOrUpdateAsync(
                tenantId, ShopNotificationType.UnpostedExpense, ShopNotificationSeverity.Information,
                "Unposted Expense",
                $"Expense \"{expense.ExpenseNumber}\" has been unposted for over {settings.DraftPendingHours} hours.",
                $"UnpostedExpense:{tenantId}:{expense.Id}",
                referenceType: "Expense", referenceId: expense.Id, referenceNumber: expense.ExpenseNumber,
                navigationUrl: ShopNotificationNavigation.Expense(), actionLabel: "ViewExpense");
        }
        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.UnpostedExpense, expensesActive);

        var stockCountsActive = new HashSet<Guid>();
        var stockCounts = await _stockCountRepository.GetListAsync(x =>
            x.TenantId == tenantId && x.Status != ShopStockCountStatus.Posted && x.Status != ShopStockCountStatus.Cancelled && x.CreationTime < cutoff);
        foreach (var count in stockCounts)
        {
            stockCountsActive.Add(count.Id);
            await _notificationManager.CreateOrUpdateAsync(
                tenantId, ShopNotificationType.StockCountPending, ShopNotificationSeverity.Information,
                "Stock Count Pending",
                $"Physical Stock Count \"{count.StockCountNumber}\" has been pending for over {settings.DraftPendingHours} hours.",
                $"StockCountPending:{tenantId}:{count.Id}",
                referenceType: "StockCount", referenceId: count.Id, referenceNumber: count.StockCountNumber,
                navigationUrl: ShopNotificationNavigation.StockCount(), actionLabel: "ViewStockCount");
        }
        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.StockCountPending, stockCountsActive);

        var adjustmentsActive = new HashSet<Guid>();
        var adjustments = await _stockAdjustmentRepository.GetListAsync(x => x.TenantId == tenantId && x.Status == ShopStockAdjustmentStatus.Draft && x.CreationTime < cutoff);
        foreach (var adjustment in adjustments)
        {
            adjustmentsActive.Add(adjustment.Id);
            await _notificationManager.CreateOrUpdateAsync(
                tenantId, ShopNotificationType.StockAdjustmentPending, ShopNotificationSeverity.Information,
                "Stock Adjustment Pending",
                $"Stock Adjustment \"{adjustment.AdjustmentNumber}\" has been pending for over {settings.DraftPendingHours} hours.",
                $"StockAdjustmentPending:{tenantId}:{adjustment.Id}",
                referenceType: "StockAdjustment", referenceId: adjustment.Id, referenceNumber: adjustment.AdjustmentNumber,
                navigationUrl: ShopNotificationNavigation.StockAdjustment(), actionLabel: "ViewStockAdjustment");
        }
        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.StockAdjustmentPending, adjustmentsActive);
    }
}
