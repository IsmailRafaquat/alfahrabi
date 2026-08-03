import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopProfitLossResultStatus {
  Profit = 0,
  BreakEven = 1,
  Loss = 2,
}

export const shopProfitLossResultStatusOptions = mapEnumToOptions(ShopProfitLossResultStatus);
