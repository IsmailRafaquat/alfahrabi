import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopCashTransactionType {
  OpeningCash = 0,
  CashSale = 1,
  CustomerPayment = 2,
  SupplierPayment = 3,
  Expense = 4,
  CustomerRefund = 5,
  CashIn = 6,
  CashOut = 7,
  ClosingAdjustment = 8,
}

export const shopCashTransactionTypeOptions = mapEnumToOptions(ShopCashTransactionType);
