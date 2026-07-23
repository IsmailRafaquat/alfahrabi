import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopSaleStatus {
  Draft = 0,
  Completed = 1,
  Cancelled = 2,
}

export const shopSaleStatusOptions = mapEnumToOptions(ShopSaleStatus);
