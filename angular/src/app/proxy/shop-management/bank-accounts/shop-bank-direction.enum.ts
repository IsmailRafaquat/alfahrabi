import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopBankDirection {
  In = 0,
  Out = 1,
}

export const shopBankDirectionOptions = mapEnumToOptions(ShopBankDirection);
