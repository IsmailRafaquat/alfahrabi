import type { ShopBankTransactionType } from '../bank-accounts/shop-bank-transaction-type.enum';
import type { ShopBankReferenceType } from '../bank-accounts/shop-bank-reference-type.enum';
import type { ShopProductBatchStatus } from '../product-batches/shop-product-batch-status.enum';
import type { ShopCashTransactionType } from '../cash-registers/shop-cash-transaction-type.enum';
import type { ShopCashReferenceType } from '../cash-registers/shop-cash-reference-type.enum';
import type { PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopCustomerLedgerReferenceType } from '../customer-ledger/shop-customer-ledger-reference-type.enum';
import type { ShopExpensePaymentMethod } from '../expenses/shop-expense-payment-method.enum';
import type { ShopExpenseStatus } from '../expenses/shop-expense-status.enum';
import type { ShopExpenseReportGroupBy } from './shop-expense-report-group-by.enum';
import type { ShopGoodsReceiptStatus } from '../goods-receipts/shop-goods-receipt-status.enum';
import type { ShopGoodsReceiptPaymentStatus } from '../goods-receipts/shop-goods-receipt-payment-status.enum';
import type { ShopSaleStatus } from '../sales/shop-sale-status.enum';
import type { ShopSalePaymentMethod } from '../sales/shop-sale-payment-method.enum';
import type { ShopSalesReportGroupBy } from './shop-sales-report-group-by.enum';
import type { ShopStockTransactionType } from '../stock-transactions/shop-stock-transaction-type.enum';
import type { ShopStockReferenceType } from '../stock-transactions/shop-stock-reference-type.enum';
import type { ShopStockQuantityDirection } from './shop-stock-quantity-direction.enum';
import type { ShopStockReportStatus } from './shop-stock-report-status.enum';
import type { ShopSupplierLedgerReferenceType } from '../supplier-ledger/shop-supplier-ledger-reference-type.enum';
import type { ShopReportPeriod } from './shop-report-period.enum';

export interface GetShopBankTransactionReportInput extends ShopReportInputBase {
  bankAccountId?: string;
  transactionType?: ShopBankTransactionType;
  referenceType?: ShopBankReferenceType;
  minimumAmount?: number;
  maximumAmount?: number;
}

export interface GetShopBatchExpiryReportInput extends ShopReportInputBase {
  productId?: string;
  productCategoryId?: string;
  supplierId?: string;
  batchStatus?: ShopProductBatchStatus;
  expiryFrom?: string;
  expiryTo?: string;
  nearExpiryOnly: boolean;
  expiredOnly: boolean;
  activeOnly: boolean;
  hasAvailableStock: boolean;
  includeBlocked: boolean;
}

export interface GetShopCashReportInput extends ShopReportInputBase {
  cashRegisterId?: string;
  transactionType?: ShopCashTransactionType;
  referenceType?: ShopCashReferenceType;
}

export interface GetShopCustomerReceivablesReportInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  customerId?: string;
  hasOutstandingBalance?: boolean;
  hasAdvanceBalance?: boolean;
  minimumBalance?: number;
  maximumBalance?: number;
  includeInactiveCustomers: boolean;
  lastTransactionFrom?: string;
  lastTransactionTo?: string;
}

export interface GetShopCustomerTransactionReportInput extends ShopReportInputBase {
  customerId?: string;
  transactionType?: ShopCustomerLedgerReferenceType;
  referenceNumber?: string;
}

export interface GetShopExpenseReportInput extends ShopReportInputBase {
  expenseCategoryId?: string;
  paymentSource?: ShopExpensePaymentMethod;
  cashRegisterId?: string;
  bankAccountId?: string;
  expenseStatus?: ShopExpenseStatus;
  minimumAmount?: number;
  maximumAmount?: number;
  createdByUserId?: string;
  groupBy?: ShopExpenseReportGroupBy;
}

export interface GetShopProductPerformanceReportInput extends ShopReportInputBase {
  productId?: string;
  productCategoryId?: string;
  supplierId?: string;
  includeInactiveProducts: boolean;
  soldOnly: boolean;
  purchasedOnly: boolean;
}

export interface GetShopPurchaseReportInput extends ShopReportInputBase {
  supplierId?: string;
  productId?: string;
  productCategoryId?: string;
  purchaseOrderId?: string;
  goodsReceiptStatus?: ShopGoodsReceiptStatus;
  paymentStatus?: ShopGoodsReceiptPaymentStatus;
  minimumAmount?: number;
  maximumAmount?: number;
  hasPendingAmount?: boolean;
}

export interface GetShopSalesReportInput extends ShopReportInputBase {
  customerId?: string;
  productId?: string;
  productCategoryId?: string;
  saleStatus?: ShopSaleStatus;
  paymentStatus?: ShopGoodsReceiptPaymentStatus;
  paymentMethod?: ShopSalePaymentMethod;
  invoiceNumber?: string;
  minimumAmount?: number;
  maximumAmount?: number;
  hasPendingAmount?: boolean;
  createdByUserId?: string;
  groupBy?: ShopSalesReportGroupBy;
}

export interface GetShopStockMovementReportInput extends ShopReportInputBase {
  productId?: string;
  productCategoryId?: string;
  productBatchId?: string;
  transactionType?: ShopStockTransactionType;
  referenceType?: ShopStockReferenceType;
  referenceNumber?: string;
  quantityDirection?: ShopStockQuantityDirection;
  createdByUserId?: string;
}

export interface GetShopStockReportInput extends ShopReportInputBase {
  productId?: string;
  productCategoryId?: string;
  unitId?: string;
  supplierId?: string;
  stockStatus?: ShopStockReportStatus;
  batchTracked?: boolean;
  expiryTracked?: boolean;
  lowStockOnly: boolean;
  outOfStockOnly: boolean;
  inStockOnly: boolean;
  includeInactiveProducts: boolean;
  minimumStock?: number;
  maximumStock?: number;
}

export interface GetShopSupplierPayablesReportInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  supplierId?: string;
  hasOutstandingBalance?: boolean;
  hasAdvanceBalance?: boolean;
  minimumBalance?: number;
  maximumBalance?: number;
  includeInactiveSuppliers: boolean;
  lastTransactionFrom?: string;
  lastTransactionTo?: string;
}

export interface GetShopSupplierTransactionReportInput extends ShopReportInputBase {
  supplierId?: string;
  transactionType?: ShopSupplierLedgerReferenceType;
  referenceNumber?: string;
}

export interface GetShopTaxSummaryReportInput {
  period?: ShopReportPeriod;
  dateFrom?: string;
  dateTo?: string;
}

export interface ShopBankTransactionReportItemDto {
  bankTransactionId?: string;
  transactionDate?: string;
  bankAccountId?: string;
  bankAccountName?: string;
  accountNumberMasked?: string;
  transactionType?: ShopBankTransactionType;
  referenceType?: ShopBankReferenceType;
  referenceId?: string;
  referenceNumber?: string;
  amountIn: number;
  amountOut: number;
  balanceAfterTransaction: number;
  description?: string;
  creationTime?: string;
}

export interface ShopBankTransactionReportResultDto {
  items: ShopBankTransactionReportItemDto[];
  totalCount: number;
  totals: ShopBankTransactionReportTotalsDto;
}

export interface ShopBankTransactionReportTotalsDto {
  openingBalance: number;
  totalAmountIn: number;
  totalAmountOut: number;
  closingBalance: number;
  transactionCount: number;
}

export interface ShopBatchExpiryReportItemDto {
  productBatchId?: string;
  productId?: string;
  productCode?: string;
  productName?: string;
  batchNumber?: string;
  manufacturingDate?: string;
  expiryDate?: string;
  daysToExpiry?: number;
  receivedQuantity: number;
  issuedQuantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  unitCost?: number;
  stockValue?: number;
  status?: ShopProductBatchStatus;
  supplierId?: string;
  supplierName?: string;
  firstReceivedDate?: string;
  lastMovementDate?: string;
  isBlocked: boolean;
  blockReason?: string;
}

export interface ShopBatchExpiryReportResultDto {
  items: ShopBatchExpiryReportItemDto[];
  totalCount: number;
  totals: ShopBatchExpiryReportTotalsDto;
}

export interface ShopBatchExpiryReportTotalsDto {
  totalBatches: number;
  activeBatches: number;
  nearExpiryBatches: number;
  expiredBatches: number;
  blockedBatches: number;
  exhaustedBatches: number;
  totalAvailableQuantity: number;
  nearExpiryQuantity: number;
  expiredQuantity: number;
  totalBatchStockValue?: number;
  expiredStockValue?: number;
}

export interface ShopCashDailySummaryDto {
  businessDate?: string;
  cashRegisterId?: string;
  cashRegisterName?: string;
  openingCash: number;
  totalCashIn: number;
  totalCashOut: number;
  expectedClosingCash: number;
  actualClosingCash?: number;
  difference?: number;
}

export interface ShopCashReportItemDto {
  cashTransactionId?: string;
  transactionDate?: string;
  cashRegisterId?: string;
  cashRegisterName?: string;
  transactionType?: ShopCashTransactionType;
  referenceType?: ShopCashReferenceType;
  referenceId?: string;
  referenceNumber?: string;
  cashIn: number;
  cashOut: number;
  balanceAfterTransaction: number;
  description?: string;
  createdByUserName?: string;
  creationTime?: string;
}

export interface ShopCashReportResultDto {
  items: ShopCashReportItemDto[];
  dailySummaries: ShopCashDailySummaryDto[];
  totalCount: number;
  totals: ShopCashReportTotalsDto;
}

export interface ShopCashReportTotalsDto {
  openingCash: number;
  totalCashIn: number;
  totalCashOut: number;
  expectedClosingCash: number;
  actualClosingCash?: number;
  difference?: number;
}

export interface ShopCustomerReceivableReportItemDto {
  customerId?: string;
  customerCode?: string;
  customerName?: string;
  phone?: string;
  totalSales: number;
  saleReturns: number;
  netSales: number;
  totalReceived: number;
  customerAdvance: number;
  outstandingBalance: number;
  lastSaleDate?: string;
  lastPaymentDate?: string;
  lastTransactionDate?: string;
  isActive: boolean;
}

export interface ShopCustomerReceivableReportResultDto {
  items: ShopCustomerReceivableReportItemDto[];
  totalCount: number;
  totals: ShopCustomerReceivableReportTotalsDto;
}

export interface ShopCustomerReceivableReportTotalsDto {
  customerCount: number;
  customersWithReceivables: number;
  customersWithAdvance: number;
  totalSales: number;
  totalSaleReturns: number;
  totalReceived: number;
  totalReceivables: number;
  totalCustomerAdvance: number;
}

export interface ShopCustomerTransactionReportItemDto {
  customerLedgerId?: string;
  transactionDate?: string;
  customerId?: string;
  customerName?: string;
  transactionType?: ShopCustomerLedgerReferenceType;
  referenceId?: string;
  referenceNumber?: string;
  debit: number;
  credit: number;
  runningBalance: number;
  description?: string;
  creationTime?: string;
}

export interface ShopCustomerTransactionReportResultDto {
  items: ShopCustomerTransactionReportItemDto[];
  totalCount: number;
  totals: ShopCustomerTransactionReportTotalsDto;
}

export interface ShopCustomerTransactionReportTotalsDto {
  openingBalance: number;
  totalDebit: number;
  totalCredit: number;
  closingBalance: number;
}

export interface ShopExpenseReportGroupItemDto {
  groupKey?: string;
  groupLabel?: string;
  expenseCount: number;
  amount: number;
  totalAmount: number;
}

export interface ShopExpenseReportItemDto {
  expenseId?: string;
  expenseNumber?: string;
  expenseDate?: string;
  expenseCategoryId?: string;
  expenseCategoryName?: string;
  description?: string;
  amount: number;
  taxAmount: number;
  totalAmount: number;
  paymentSource?: ShopExpensePaymentMethod;
  bankAccountId?: string;
  bankAccountName?: string;
  referenceNumber?: string;
  status?: ShopExpenseStatus;
  createdByUserName?: string;
  creationTime?: string;
}

export interface ShopExpenseReportResultDto {
  items: ShopExpenseReportItemDto[];
  groups: ShopExpenseReportGroupItemDto[];
  totalCount: number;
  totals: ShopExpenseReportTotalsDto;
}

export interface ShopExpenseReportTotalsDto {
  expenseCount: number;
  expenseAmount: number;
  taxAmount: number;
  totalExpenseAmount: number;
  cashExpenses: number;
  bankExpenses: number;
  otherSourceExpenses: number;
  averageExpense: number;
}

export interface ShopProductPerformanceReportItemDto {
  productId?: string;
  productCode?: string;
  productName?: string;
  categoryName?: string;
  unitName?: string;
  openingStock: number;
  purchasedQuantity: number;
  purchaseReturnQuantity: number;
  saleQuantity: number;
  saleReturnQuantity: number;
  adjustmentInQuantity: number;
  adjustmentOutQuantity: number;
  closingStock: number;
  grossSalesAmount: number;
  saleReturnAmount: number;
  netSalesAmount: number;
  purchaseAmount?: number;
  purchaseReturnAmount?: number;
  netPurchaseAmount?: number;
  currentStock: number;
  currentStockValue?: number;
  averageSalePrice?: number;
  averagePurchaseCost?: number;
}

export interface ShopProductPerformanceReportResultDto {
  items: ShopProductPerformanceReportItemDto[];
  totalCount: number;
  totals: ShopProductPerformanceReportTotalsDto;
}

export interface ShopProductPerformanceReportTotalsDto {
  productCount: number;
  totalPurchasedQuantity: number;
  totalSaleQuantity: number;
  totalNetSalesAmount: number;
  totalNetPurchaseAmount?: number;
  totalCurrentStock: number;
  totalCurrentStockValue?: number;
}

export interface ShopPurchaseReportItemDto {
  goodsReceiptId?: string;
  goodsReceiptNumber?: string;
  goodsReceiptDate?: string;
  purchaseOrderId?: string;
  purchaseOrderNumber?: string;
  supplierId?: string;
  supplierName?: string;
  grossAmount: number;
  discountAmount: number;
  taxAmount: number;
  netAmount: number;
  paidAmount: number;
  pendingAmount: number;
  purchaseReturnAmount: number;
  finalPurchaseAmount: number;
  totalItems: number;
  totalQuantity: number;
  status?: ShopGoodsReceiptStatus;
  paymentStatus?: ShopGoodsReceiptPaymentStatus;
  creationTime?: string;
}

export interface ShopPurchaseReportResultDto {
  items: ShopPurchaseReportItemDto[];
  totalCount: number;
  totals: ShopPurchaseReportTotalsDto;
}

export interface ShopPurchaseReportTotalsDto {
  purchaseCount: number;
  grossPurchases: number;
  totalDiscount: number;
  totalTax: number;
  netPurchasesBeforeReturns: number;
  purchaseReturnAmount: number;
  finalNetPurchases: number;
  paidAmount: number;
  pendingAmount: number;
  totalQuantityPurchased: number;
  averagePurchaseValue: number;
}

export interface ShopReportInputBase extends PagedAndSortedResultRequestDto {
  period?: ShopReportPeriod;
  dateFrom?: string;
  dateTo?: string;
  filter?: string;
}

export interface ShopSalesReportGroupItemDto {
  groupKey?: string;
  groupLabel?: string;
  saleCount: number;
  grossAmount: number;
  discountAmount: number;
  taxAmount: number;
  netAmount: number;
  returnAmount: number;
  finalSalesAmount: number;
  totalQuantity: number;
}

export interface ShopSalesReportItemDto {
  saleId?: string;
  invoiceNumber?: string;
  saleDate?: string;
  customerId?: string;
  customerName?: string;
  customerPhone?: string;
  grossAmount: number;
  discountAmount: number;
  taxAmount: number;
  netAmount: number;
  paidAmount: number;
  pendingAmount: number;
  returnAmount: number;
  finalSalesAmount: number;
  paymentStatus?: ShopGoodsReceiptPaymentStatus;
  saleStatus?: ShopSaleStatus;
  paymentMethod?: ShopSalePaymentMethod;
  totalItems: number;
  totalQuantity: number;
  createdByUserName?: string;
  creationTime?: string;
}

export interface ShopSalesReportResultDto {
  items: ShopSalesReportItemDto[];
  groups: ShopSalesReportGroupItemDto[];
  totalCount: number;
  totals: ShopSalesReportTotalsDto;
}

export interface ShopSalesReportTotalsDto {
  saleCount: number;
  grossSales: number;
  totalDiscount: number;
  totalTax: number;
  netSalesBeforeReturns: number;
  saleReturnAmount: number;
  finalNetSales: number;
  paidAmount: number;
  pendingAmount: number;
  totalQuantitySold: number;
  averageSaleValue: number;
}

export interface ShopStockMovementReportItemDto {
  stockTransactionId?: string;
  transactionDate?: string;
  productId?: string;
  productCode?: string;
  productName?: string;
  unitName?: string;
  productBatchId?: string;
  batchNumber?: string;
  expiryDate?: string;
  transactionType?: ShopStockTransactionType;
  referenceType?: ShopStockReferenceType;
  referenceId?: string;
  referenceNumber?: string;
  quantityIn: number;
  quantityOut: number;
  productBalanceQuantity: number;
  batchBalanceQuantity?: number;
  unitCost?: number;
  totalCost?: number;
  notes?: string;
  createdByUserName?: string;
  creationTime?: string;
}

export interface ShopStockMovementReportResultDto {
  items: ShopStockMovementReportItemDto[];
  totalCount: number;
  totals: ShopStockMovementReportTotalsDto;
}

export interface ShopStockMovementReportTotalsDto {
  transactionCount: number;
  totalQuantityIn: number;
  totalQuantityOut: number;
  netQuantityMovement: number;
  totalStockInValue?: number;
  totalStockOutValue?: number;
}

export interface ShopStockReportItemDto {
  productId?: string;
  productCode?: string;
  barcode?: string;
  productName?: string;
  categoryId?: string;
  categoryName?: string;
  unitId?: string;
  unitName?: string;
  unitShortName?: string;
  currentStock: number;
  reorderLevel: number;
  stockStatus?: ShopStockReportStatus;
  purchasePrice?: number;
  averageCost?: number;
  salePrice: number;
  stockValue?: number;
  potentialSaleValue: number;
  potentialMargin?: number;
  trackBatch: boolean;
  trackExpiry: boolean;
  activeBatchCount: number;
  nearExpiryQuantity: number;
  expiredQuantity: number;
  isActive: boolean;
}

export interface ShopStockReportResultDto {
  items: ShopStockReportItemDto[];
  totalCount: number;
  totals: ShopStockReportTotalsDto;
}

export interface ShopStockReportTotalsDto {
  totalProducts: number;
  activeProducts: number;
  inStockProducts: number;
  lowStockProducts: number;
  outOfStockProducts: number;
  negativeStockProducts: number;
  totalStockQuantity: number;
  totalStockValue?: number;
  totalPotentialSaleValue: number;
  totalPotentialMargin?: number;
}

export interface ShopSupplierPayableReportItemDto {
  supplierId?: string;
  supplierCode?: string;
  supplierName?: string;
  phone?: string;
  totalPurchases: number;
  purchaseReturns: number;
  netPurchases: number;
  totalPaid: number;
  supplierAdvance: number;
  outstandingBalance: number;
  lastPurchaseDate?: string;
  lastPaymentDate?: string;
  lastTransactionDate?: string;
  isActive: boolean;
}

export interface ShopSupplierPayableReportResultDto {
  items: ShopSupplierPayableReportItemDto[];
  totalCount: number;
  totals: ShopSupplierPayableReportTotalsDto;
}

export interface ShopSupplierPayableReportTotalsDto {
  supplierCount: number;
  suppliersWithPayables: number;
  suppliersWithAdvance: number;
  totalPurchases: number;
  totalPurchaseReturns: number;
  totalPaid: number;
  totalPayables: number;
  totalSupplierAdvance: number;
}

export interface ShopSupplierTransactionReportItemDto {
  supplierLedgerId?: string;
  transactionDate?: string;
  supplierId?: string;
  supplierName?: string;
  transactionType?: ShopSupplierLedgerReferenceType;
  referenceId?: string;
  referenceNumber?: string;
  debit: number;
  credit: number;
  runningBalance: number;
  description?: string;
  creationTime?: string;
}

export interface ShopSupplierTransactionReportResultDto {
  items: ShopSupplierTransactionReportItemDto[];
  totalCount: number;
  totals: ShopSupplierTransactionReportTotalsDto;
}

export interface ShopSupplierTransactionReportTotalsDto {
  openingBalance: number;
  totalDebit: number;
  totalCredit: number;
  closingBalance: number;
}

export interface ShopTaxSummaryReportDto {
  dateFrom?: string;
  dateTo?: string;
  salesTaxCollected: number;
  salesReturnTaxReversed: number;
  netSalesTax: number;
  purchaseTaxPaid: number;
  purchaseReturnTaxReversed: number;
  netPurchaseTax: number;
  expenseTaxPaid: number;
  netTaxPosition: number;
}
