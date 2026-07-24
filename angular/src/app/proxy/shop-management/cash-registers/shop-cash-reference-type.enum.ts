import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopCashReferenceType {
  CashClosing = 0,
  Sale = 1,
  CustomerPayment = 2,
  SupplierPayment = 3,
  Expense = 4,
  SaleReturn = 5,
  ManualCashMovement = 6,
}

export const shopCashReferenceTypeOptions = mapEnumToOptions(ShopCashReferenceType);
