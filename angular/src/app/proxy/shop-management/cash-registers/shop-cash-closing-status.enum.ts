import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopCashClosingStatus {
  Open = 0,
  Closed = 1,
  Cancelled = 2,
}

export const shopCashClosingStatusOptions = mapEnumToOptions(ShopCashClosingStatus);
