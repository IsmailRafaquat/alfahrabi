import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopReportPeriod {
  Today = 0,
  Yesterday = 1,
  Last7Days = 2,
  Last30Days = 3,
  ThisMonth = 4,
  LastMonth = 5,
  ThisYear = 6,
  Custom = 7,
  ThisQuarter = 8,
}

export const shopReportPeriodOptions = mapEnumToOptions(ShopReportPeriod);
