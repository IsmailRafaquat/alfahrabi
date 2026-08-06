import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopCustomerPaymentType {
  InvoicePayment = 0,
  Advance = 1,
  OnAccount = 2,
  Refund = 3,
  Adjustment = 4,
}

export const shopCustomerPaymentTypeOptions = mapEnumToOptions(ShopCustomerPaymentType);
