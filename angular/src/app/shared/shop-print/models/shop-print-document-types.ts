// Mirrors EHub.Application.Contracts/ShopManagement/PrintTemplates/ShopPrintDocumentTypes.cs
// EXACTLY - must be kept in sync by hand with the backend const class. This is the frontend-side
// half of the allowlist: the print-preview route rejects any documentType not in this list
// BEFORE ever calling the API, as defense in depth alongside the backend's own allowlist check.
export const SHOP_PRINT_DOCUMENT_TYPES = {
  Sale: 'sale',
  SaleReturn: 'sale-return',
  PurchaseOrder: 'purchase-order',
  GoodsReceipt: 'goods-receipt',
  PurchaseReturn: 'purchase-return',
  CustomerPayment: 'customer-payment',
  SupplierPayment: 'supplier-payment',
  CustomerLedgerStatement: 'customer-ledger-statement',
  SupplierLedgerStatement: 'supplier-ledger-statement',
  ExpenseVoucher: 'expense-voucher',
  CashClosingSlip: 'cash-closing-slip',
  BankTransaction: 'bank-transaction',
  BankTransfer: 'bank-transfer',
  StockAdjustment: 'stock-adjustment',
  StockCount: 'stock-count',
  StockTransaction: 'stock-transaction',
  ProductBarcodeLabel: 'product-barcode-label',
  BatchExpiryLabel: 'batch-expiry-label',
} as const;

export type ShopPrintDocumentType = (typeof SHOP_PRINT_DOCUMENT_TYPES)[keyof typeof SHOP_PRINT_DOCUMENT_TYPES];

export const SHOP_PRINT_DOCUMENT_TYPE_LIST: string[] = Object.values(SHOP_PRINT_DOCUMENT_TYPES);

export function isValidShopPrintDocumentType(value: string | null | undefined): value is ShopPrintDocumentType {
  return !!value && SHOP_PRINT_DOCUMENT_TYPE_LIST.includes(value);
}

// Document types that render as a fixed-size label (grid of labels) rather than a receipt.
export const SHOP_PRINT_LABEL_DOCUMENT_TYPES: string[] = [
  SHOP_PRINT_DOCUMENT_TYPES.ProductBarcodeLabel,
  SHOP_PRINT_DOCUMENT_TYPES.BatchExpiryLabel,
];
