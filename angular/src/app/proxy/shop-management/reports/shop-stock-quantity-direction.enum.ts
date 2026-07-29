import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopStockQuantityDirection {
  All = 0,
  StockIn = 1,
  StockOut = 2,
}

export const shopStockQuantityDirectionOptions = mapEnumToOptions(ShopStockQuantityDirection);
