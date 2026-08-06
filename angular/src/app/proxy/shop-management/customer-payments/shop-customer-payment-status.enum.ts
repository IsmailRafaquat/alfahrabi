import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopCustomerPaymentStatus {
  Draft = 0,
  Posted = 1,
  Cancelled = 2,
}

export const shopCustomerPaymentStatusOptions = mapEnumToOptions(ShopCustomerPaymentStatus);
