import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopSupplierPaymentStatus {
  Draft = 0,
  Posted = 1,
  Cancelled = 2,
}

export const shopSupplierPaymentStatusOptions = mapEnumToOptions(ShopSupplierPaymentStatus);
