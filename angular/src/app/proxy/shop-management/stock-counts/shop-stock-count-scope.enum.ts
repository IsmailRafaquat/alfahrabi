import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopStockCountScope {
  AllProducts = 0,
  SelectedProducts = 1,
  Category = 2,
}

export const shopStockCountScopeOptions = mapEnumToOptions(ShopStockCountScope);
