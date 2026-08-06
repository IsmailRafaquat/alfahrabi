import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopReportExportFormat {
  Excel = 0,
  Pdf = 1,
  Csv = 2,
}

export const shopReportExportFormatOptions = mapEnumToOptions(ShopReportExportFormat);
