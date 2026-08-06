import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopThermalFontSize {
  Small = 0,
  Medium = 1,
  Large = 2,
}

export const shopThermalFontSizeOptions = mapEnumToOptions(ShopThermalFontSize);
