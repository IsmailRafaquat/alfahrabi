import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopPurchaseReturnStatus {
  Draft = 0,
  Completed = 1,
  Cancelled = 2,
}

export const shopPurchaseReturnStatusOptions = mapEnumToOptions(ShopPurchaseReturnStatus);
