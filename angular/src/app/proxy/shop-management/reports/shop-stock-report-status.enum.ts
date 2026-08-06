import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopStockReportStatus {
  All = 0,
  InStock = 1,
  LowStock = 2,
  OutOfStock = 3,
  NegativeStock = 4,
}

export const shopStockReportStatusOptions = mapEnumToOptions(ShopStockReportStatus);
