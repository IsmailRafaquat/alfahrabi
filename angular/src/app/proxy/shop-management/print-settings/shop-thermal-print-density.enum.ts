import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopThermalPrintDensity {
  Light = 0,
  Normal = 1,
  Dark = 2,
}

export const shopThermalPrintDensityOptions = mapEnumToOptions(ShopThermalPrintDensity);
