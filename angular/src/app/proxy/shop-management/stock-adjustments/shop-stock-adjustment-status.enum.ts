import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopStockAdjustmentStatus {
  Draft = 0,
  Posted = 1,
  Cancelled = 2,
}

export const shopStockAdjustmentStatusOptions = mapEnumToOptions(ShopStockAdjustmentStatus);
