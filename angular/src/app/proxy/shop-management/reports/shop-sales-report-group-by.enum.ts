import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopSalesReportGroupBy {
  None = 0,
  Day = 1,
  Month = 2,
  Customer = 3,
  Product = 4,
  Category = 5,
  PaymentMethod = 6,
}

export const shopSalesReportGroupByOptions = mapEnumToOptions(ShopSalesReportGroupBy);
