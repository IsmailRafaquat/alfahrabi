import { mapEnumToOptions } from '@abp/ng.core';

export enum FeeHeadChargeType {
  Monthly = 1,
  OneTimePerSession = 2,
}

export const feeHeadChargeTypeOptions = mapEnumToOptions(FeeHeadChargeType);
