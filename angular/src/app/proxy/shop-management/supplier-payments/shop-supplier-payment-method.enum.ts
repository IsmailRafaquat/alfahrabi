import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopSupplierPaymentMethod {
  Cash = 0,
  BankTransfer = 1,
  Cheque = 2,
  Card = 3,
  Other = 4,
}

export const shopSupplierPaymentMethodOptions = mapEnumToOptions(ShopSupplierPaymentMethod);
