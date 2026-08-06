import type { ShopDashboardPeriod } from './shop-dashboard-period.enum';
import type { ShopProductBatchStatus } from '../product-batches/shop-product-batch-status.enum';
import type { ShopExpensePaymentMethod } from '../expenses/shop-expense-payment-method.enum';
import type { ShopExpenseStatus } from '../expenses/shop-expense-status.enum';
import type { ShopGoodsReceiptPaymentStatus } from '../goods-receipts/shop-goods-receipt-payment-status.enum';
import type { ShopSaleStatus } from '../sales/shop-sale-status.enum';

export interface GetShopDashboardInput {
  period?: ShopDashboardPeriod;
  dateFrom?: string;
  dateTo?: string;
  topProductsCount: number;
  recentItemsCount: number;
}

export interface ShopDashboardBalanceSummaryDto {
  cashBalance: number;
  bankBalance: number;
  customerReceivables: number;
  supplierPayables: number;
  customerAdvanceBalance: number;
  supplierAdvanceBalance: number;
}

export interface ShopDashboardBatchAlertDto {
  productId?: string;
  productCode?: string;
  productName?: string;
  productBatchId?: string;
  batchNumber?: string;
  expiryDate?: string;
  daysToExpiry?: number;
  availableQuantity: number;
  status?: ShopProductBatchStatus;
}

export interface ShopDashboardChartPointDto {
  label?: string;
  periodStart?: string;
  value: number;
}

export interface ShopDashboardDto {
  period?: ShopDashboardPeriod;
  dateFrom?: string;
  dateTo?: string;
  shopDisplayName?: string;
  currencyCode?: string;
  currencySymbol?: string;
  decimalPlaces: number;
  summary: ShopDashboardSummaryDto;
  salesExpenseChart: ShopDashboardSalesExpenseChartDto;
  salesTrend: ShopDashboardSalesTrendDto;
  topProducts: ShopDashboardTopProductDto[];
  lowStockProducts: ShopDashboardLowStockProductDto[];
  nearExpiryBatches: ShopDashboardBatchAlertDto[];
  expiredBatches: ShopDashboardBatchAlertDto[];
  balances: ShopDashboardBalanceSummaryDto;
  recentSales: ShopDashboardRecentSaleDto[];
  recentPurchases: ShopDashboardRecentPurchaseDto[];
  recentExpenses: ShopDashboardRecentExpenseDto[];
  inventory: ShopDashboardInventorySummaryDto;
}

export interface ShopDashboardInventorySummaryDto {
  inventoryQuantity: number;
  inventoryValue?: number;
  lowStockProductCount: number;
  outOfStockProductCount: number;
  nearExpiryBatchCount: number;
  expiredBatchCount: number;
}

export interface ShopDashboardLowStockProductDto {
  productId?: string;
  productCode?: string;
  productName?: string;
  unitName?: string;
  currentStock: number;
  reorderLevel: number;
  requiredReorderQuantity: number;
  isOutOfStock: boolean;
}

export interface ShopDashboardRecentExpenseDto {
  expenseId?: string;
  expenseNumber?: string;
  expenseDate?: string;
  expenseCategoryName?: string;
  description?: string;
  amount: number;
  paymentSource?: ShopExpensePaymentMethod;
  status?: ShopExpenseStatus;
}

export interface ShopDashboardRecentPurchaseDto {
  purchaseId?: string;
  documentNumber?: string;
  documentDate?: string;
  supplierId?: string;
  supplierName?: string;
  totalAmount: number;
  paidAmount: number;
  pendingAmount: number;
  status?: ShopGoodsReceiptPaymentStatus;
}

export interface ShopDashboardRecentSaleDto {
  saleId?: string;
  invoiceNumber?: string;
  saleDate?: string;
  customerId?: string;
  customerName?: string;
  totalAmount: number;
  paidAmount: number;
  pendingAmount: number;
  paymentStatus?: ShopGoodsReceiptPaymentStatus;
  saleStatus?: ShopSaleStatus;
}

export interface ShopDashboardSalesExpenseChartDto {
  groupBy?: string;
  netSalesPoints: ShopDashboardChartPointDto[];
  expensePoints: ShopDashboardChartPointDto[];
}

export interface ShopDashboardSalesTrendDto {
  currentPeriodNetSales: number;
  previousPeriodNetSales: number;
  salesGrowthAmount: number;
  salesGrowthPercentage?: number;
}

export interface ShopDashboardSummaryDto {
  totalSales?: number;
  totalSalesCount?: number;
  grossSales?: number;
  saleReturns?: number;
  netSales?: number;
  totalPurchases?: number;
  totalPurchasesCount?: number;
  purchaseReturns?: number;
  netPurchases?: number;
  totalExpenses?: number;
  totalExpensesCount?: number;
  customerReceivables?: number;
  supplierPayables?: number;
  cashBalance?: number;
  bankBalance?: number;
  totalAvailableBalance?: number;
  inventoryQuantity: number;
  inventoryValue?: number;
  lowStockProductCount?: number;
  outOfStockProductCount?: number;
  nearExpiryBatchCount?: number;
  expiredBatchCount?: number;
  activeCustomerCount: number;
  activeSupplierCount: number;
}

export interface ShopDashboardTopProductDto {
  productId?: string;
  productCode?: string;
  productName?: string;
  unitName?: string;
  quantitySold: number;
  grossSalesAmount: number;
  returnQuantity: number;
  netQuantitySold: number;
  netSalesAmount: number;
}
