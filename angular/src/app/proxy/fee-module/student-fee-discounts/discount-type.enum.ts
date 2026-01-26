import { mapEnumToOptions } from '@abp/ng.core';

export enum DiscountType {
  Fixed = 1,
  Percent = 2,
}

export const discountTypeOptions = mapEnumToOptions(DiscountType);
