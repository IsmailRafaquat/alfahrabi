import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopSupplierLedgerReferenceType {
  OpeningBalance = 0,
  GoodsReceipt = 1,
  SupplierPayment = 2,
  PurchaseReturn = 3,
}

export const shopSupplierLedgerReferenceTypeOptions = mapEnumToOptions(ShopSupplierLedgerReferenceType);
