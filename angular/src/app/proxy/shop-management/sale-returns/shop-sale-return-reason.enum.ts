import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopSaleReturnReason {
  Damaged = 0,
  WrongProduct = 1,
  QualityIssue = 2,
  CustomerChangedMind = 3,
  ExcessQuantity = 4,
  Expired = 5,
  Other = 6,
}

export const shopSaleReturnReasonOptions = mapEnumToOptions(ShopSaleReturnReason);
