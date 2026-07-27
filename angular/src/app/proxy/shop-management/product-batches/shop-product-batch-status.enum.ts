import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopProductBatchStatus {
  Active = 0,
  NearExpiry = 1,
  Expired = 2,
  Exhausted = 3,
  Blocked = 4,
}

export const shopProductBatchStatusOptions = mapEnumToOptions(ShopProductBatchStatus);
