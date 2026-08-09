import type { ShopReportPeriod } from '../reports/shop-report-period.enum';
import type { ShopProfitLossResultStatus } from './shop-profit-loss-result-status.enum';

export interface GetShopProfitLossInput {
  period?: ShopReportPeriod;
  dateFrom?: string;
  dateTo?: string;
  compareWithPreviousPeriod: boolean;
  includeExpenseBreakdown: boolean;
  includeProductContribution: boolean;
  topProductCount: number;
}

export interface ShopProfitLossComparisonDto {
  currentNetSales: number;
  previousNetSales: number;
  currentGrossProfit?: number;
  previousGrossProfit?: number;
  currentNetProfit?: number;
  previousNetProfit?: number;
  netProfitChangeAmount?: number;
  netProfitChangePercentage?: number;
}

export interface ShopProfitLossDto {
  summary: ShopProfitLossSummaryDto;
  trend: ShopProfitLossTrendPointDto[];
  expenseBreakdown: ShopProfitLossExpenseCategoryDto[];
  productContribution: ShopProfitLossProductContributionDto[];
  comparison: ShopProfitLossComparisonDto;
}

export interface ShopProfitLossExpenseCategoryDto {
  expenseCategoryId?: string;
  expenseCategoryName?: string;
  amount: number;
  percentageOfTotalExpenses: number;
  transactionCount: number;
}

export interface ShopProfitLossProductContributionDto {
  productId?: string;
  productCode?: string;
  productName?: string;
  quantitySold: number;
  netSales: number;
  costOfGoodsSold?: number;
  grossProfit?: number;
  grossMarginPercentage?: number;
}

export interface ShopProfitLossSummaryDto {
  grossSales: number;
  salesDiscounts: number;
  salesTax: number;
  salesReturns: number;
  netSales: number;
  costOfGoodsSoldBeforeReturns?: number;
  returnedCostOfGoodsSold?: number;
  costOfGoodsSold?: number;
  grossProfit?: number;
  grossProfitMarginPercentage?: number;
  operatingExpenses?: number;
  otherIncome: number;
  netProfit?: number;
  netProfitMarginPercentage?: number;
  resultStatus?: ShopProfitLossResultStatus;
  openingInventoryValue?: number;
  netPurchases?: number;
  closingInventoryValue?: number;
  currentPeriodFrom?: string;
  currentPeriodTo?: string;
  currencyCode?: string;
  currencySymbol?: string;
}

export interface ShopProfitLossTrendPointDto {
  label?: string;
  periodStart?: string;
  netSales: number;
  costOfGoodsSold?: number;
  grossProfit?: number;
  operatingExpenses?: number;
  netProfit?: number;
}
