import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopSupplierPaymentType {
  Advance = 0,
  InvoicePayment = 1,
  OnAccount = 2,
  Refund = 3,
  Adjustment = 4,
}

export const shopSupplierPaymentTypeOptions = mapEnumToOptions(ShopSupplierPaymentType);
