import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopStockAdjustmentType {
  Increase = 0,
  Decrease = 1,
}

export const shopStockAdjustmentTypeOptions = mapEnumToOptions(ShopStockAdjustmentType);
