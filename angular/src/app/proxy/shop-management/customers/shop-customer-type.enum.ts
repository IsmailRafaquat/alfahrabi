import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopCustomerType {
  Individual = 0,
  Business = 1,
  WalkIn = 2,
}

export const shopCustomerTypeOptions = mapEnumToOptions(ShopCustomerType);
