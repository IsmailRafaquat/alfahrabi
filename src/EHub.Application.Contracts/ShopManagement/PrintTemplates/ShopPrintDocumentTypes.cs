namespace EHub.ShopManagement.PrintTemplates;

// Backend allowlist of printable document types. The Angular frontend keeps its own mirrored
// constant array (shop-print-document-types.ts) for client-side validation before ever calling
// the API - both lists must be kept in sync by hand since these values cannot be shared directly
// across the .NET/TypeScript boundary.
public static class ShopPrintDocumentTypes
{
    public const string Sale = "sale";
    public const string SaleReturn = "sale-return";
    public const string PurchaseOrder = "purchase-order";
    public const string GoodsReceipt = "goods-receipt";
    public const string PurchaseReturn = "purchase-return";
    public const string CustomerPayment = "customer-payment";
    public const string SupplierPayment = "supplier-payment";
    public const string CustomerLedgerStatement = "customer-ledger-statement";
    public const string SupplierLedgerStatement = "supplier-ledger-statement";
    public const string ExpenseVoucher = "expense-voucher";
    public const string CashClosingSlip = "cash-closing-slip";
    public const string BankTransaction = "bank-transaction";
    public const string BankTransfer = "bank-transfer";
    public const string StockAdjustment = "stock-adjustment";
    public const string StockCount = "stock-count";
    public const string StockTransaction = "stock-transaction";
    public const string ProductBarcodeLabel = "product-barcode-label";
    public const string BatchExpiryLabel = "batch-expiry-label";

    public static readonly string[] All =
    {
        Sale, SaleReturn, PurchaseOrder, GoodsReceipt, PurchaseReturn,
        CustomerPayment, SupplierPayment, CustomerLedgerStatement, SupplierLedgerStatement,
        ExpenseVoucher, CashClosingSlip, BankTransaction, BankTransfer,
        StockAdjustment, StockCount, StockTransaction,
        ProductBarcodeLabel, BatchExpiryLabel
    };
}
