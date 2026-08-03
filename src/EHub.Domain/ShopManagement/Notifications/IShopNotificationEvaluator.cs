using System;
using System.Threading.Tasks;
using EHub.ShopManagement.CashRegisters;
using EHub.ShopManagement.Notifications.Generators;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.Notifications;

/// <summary>
/// Thin facade over the narrow "evaluate one entity" generator methods, for the small set of
/// immediate hooks wired into existing domain managers. Every call site wraps these in try/catch -
/// a notification failure must never break the underlying business transaction.
/// </summary>
public interface IShopNotificationEvaluator
{
    Task EvaluateProductStockAsync(Guid tenantId, Guid productId);
    Task ResolveCustomerOverdueAsync(Guid tenantId, Guid customerId);
    Task ResolveSupplierOverdueAsync(Guid tenantId, Guid supplierId);
    Task EvaluateCashClosingAsync(Guid tenantId, ShopCashClosing closing);
}

public class ShopNotificationEvaluator : IShopNotificationEvaluator, ITransientDependency
{
    private readonly LowStockNotificationGenerator _lowStockGenerator;
    private readonly CustomerDueNotificationGenerator _customerDueGenerator;
    private readonly SupplierDueNotificationGenerator _supplierDueGenerator;
    private readonly CashDifferenceNotificationGenerator _cashDifferenceGenerator;

    public ShopNotificationEvaluator(
        LowStockNotificationGenerator lowStockGenerator,
        CustomerDueNotificationGenerator customerDueGenerator,
        SupplierDueNotificationGenerator supplierDueGenerator,
        CashDifferenceNotificationGenerator cashDifferenceGenerator)
    {
        _lowStockGenerator = lowStockGenerator;
        _customerDueGenerator = customerDueGenerator;
        _supplierDueGenerator = supplierDueGenerator;
        _cashDifferenceGenerator = cashDifferenceGenerator;
    }

    public Task EvaluateProductStockAsync(Guid tenantId, Guid productId) =>
        _lowStockGenerator.EvaluateProductAsync(tenantId, productId);

    public Task ResolveCustomerOverdueAsync(Guid tenantId, Guid customerId) =>
        _customerDueGenerator.ResolveCustomerAsync(tenantId, customerId);

    public Task ResolveSupplierOverdueAsync(Guid tenantId, Guid supplierId) =>
        _supplierDueGenerator.ResolveSupplierAsync(tenantId, supplierId);

    public Task EvaluateCashClosingAsync(Guid tenantId, ShopCashClosing closing) =>
        _cashDifferenceGenerator.EvaluateClosingAsync(tenantId, closing);
}
