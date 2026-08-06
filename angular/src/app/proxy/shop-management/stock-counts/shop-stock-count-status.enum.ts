import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopStockCountStatus {
  Draft = 0,
  InProgress = 1,
  Counted = 2,
  Posted = 3,
  Cancelled = 4,
}

export const shopStockCountStatusOptions = mapEnumToOptions(ShopStockCountStatus);
