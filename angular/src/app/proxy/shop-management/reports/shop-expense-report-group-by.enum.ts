import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopExpenseReportGroupBy {
  None = 0,
  Day = 1,
  Month = 2,
  ExpenseCategory = 3,
  PaymentSource = 4,
  User = 5,
}

export const shopExpenseReportGroupByOptions = mapEnumToOptions(ShopExpenseReportGroupBy);
