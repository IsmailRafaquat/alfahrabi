import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopCashDirection {
  In = 0,
  Out = 1,
}

export const shopCashDirectionOptions = mapEnumToOptions(ShopCashDirection);
