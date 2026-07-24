import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopSaleReturnStatus {
  Draft = 0,
  Completed = 1,
  Cancelled = 2,
}

export const shopSaleReturnStatusOptions = mapEnumToOptions(ShopSaleReturnStatus);
