import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopGoodsReceiptStatus {
  Draft = 0,
  Completed = 1,
  Cancelled = 2,
}

export const shopGoodsReceiptStatusOptions = mapEnumToOptions(ShopGoodsReceiptStatus);
