import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopSaleType {
  Cash = 0,
  Credit = 1,
}

export const shopSaleTypeOptions = mapEnumToOptions(ShopSaleType);
