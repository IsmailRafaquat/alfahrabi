import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopPurchaseReturnReason {
  Damaged = 0,
  Expired = 1,
  WrongProduct = 2,
  QualityIssue = 3,
  ExcessQuantity = 4,
  Other = 5,
}

export const shopPurchaseReturnReasonOptions = mapEnumToOptions(ShopPurchaseReturnReason);
