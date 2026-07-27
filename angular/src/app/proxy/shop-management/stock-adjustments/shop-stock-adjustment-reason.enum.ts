import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopStockAdjustmentReason {
  Damaged = 0,
  Expired = 1,
  Lost = 2,
  Stolen = 3,
  ExtraStockFound = 4,
  CountingCorrection = 5,
  OpeningStockCorrection = 6,
  Other = 7,
}

export const shopStockAdjustmentReasonOptions = mapEnumToOptions(ShopStockAdjustmentReason);
