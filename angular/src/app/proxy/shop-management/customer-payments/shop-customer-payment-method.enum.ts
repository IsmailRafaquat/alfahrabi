import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopCustomerPaymentMethod {
  Cash = 0,
  BankTransfer = 1,
  Card = 2,
  Cheque = 3,
  Other = 4,
}

export const shopCustomerPaymentMethodOptions = mapEnumToOptions(ShopCustomerPaymentMethod);
