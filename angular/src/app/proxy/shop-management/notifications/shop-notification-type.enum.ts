import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopNotificationType {
  LowStock = 0,
  OutOfStock = 1,
  NearExpiry = 2,
  ExpiredBatch = 3,
  CustomerPaymentDue = 4,
  SupplierPaymentDue = 5,
  CustomerOverdueBalance = 6,
  SupplierOverdueBalance = 7,
  CashClosingDifference = 8,
  BankLowBalance = 9,
  DraftSalePending = 10,
  DraftPurchasePending = 11,
  UnpostedExpense = 12,
  StockCountPending = 13,
  StockAdjustmentPending = 14,
  ProfitLossWarning = 15,
  SystemInformation = 16,
}

export const shopNotificationTypeOptions = mapEnumToOptions(ShopNotificationType);
