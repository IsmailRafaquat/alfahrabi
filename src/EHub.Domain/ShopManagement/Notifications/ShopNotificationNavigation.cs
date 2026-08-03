using System;

namespace EHub.ShopManagement.Notifications;

/// <summary>
/// Every NavigationUrl stored on a notification comes from here - a fixed set of route templates
/// with only a Guid interpolated. Generators and hooks never accept or forward a caller-supplied
/// URL, so a notification can never be made to point somewhere arbitrary.
/// </summary>
internal static class ShopNotificationNavigation
{
    public static string Product(Guid productId) => $"/shop-management/products?highlight={productId}";
    public static string ProductBatch(Guid productId) => $"/shop-management/product-batches?productId={productId}";
    public static string CustomerLedger(Guid customerId) => $"/shop-management/customer-ledger?customerId={customerId}";
    public static string SupplierLedger(Guid supplierId) => $"/shop-management/supplier-ledger?supplierId={supplierId}";
    public static string CashClosing(Guid cashClosingId) => $"/shop-management/cash-register/closings?id={cashClosingId}";
    public static string BankAccount(Guid bankAccountId) => $"/shop-management/bank-accounts?id={bankAccountId}";
    public static string Sale(Guid saleId) => $"/shop-management/sales/edit/{saleId}";
    public static string PurchaseOrder(Guid purchaseOrderId) => $"/shop-management/purchase-orders/edit/{purchaseOrderId}";
    public static string Expense() => "/shop-management/expenses";
    public static string StockCount() => "/shop-management/stock-counts";
    public static string StockAdjustment() => "/shop-management/stock-adjustments";
    public static string ProfitLoss() => "/shop-management/reports/profit-loss";
}
