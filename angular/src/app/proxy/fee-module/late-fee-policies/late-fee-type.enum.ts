import { mapEnumToOptions } from '@abp/ng.core';

export enum LateFeeType {
  FixedOnce = 1,
  FixedPerDay = 2,
}

export const lateFeeTypeOptions = mapEnumToOptions(LateFeeType);
