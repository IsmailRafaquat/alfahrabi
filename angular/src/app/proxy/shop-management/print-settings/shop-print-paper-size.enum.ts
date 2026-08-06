import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopPrintPaperSize {
  Thermal58Mm = 0,
  Thermal80Mm = 1,
  A4 = 2,
}

export const shopPrintPaperSizeOptions = mapEnumToOptions(ShopPrintPaperSize);
