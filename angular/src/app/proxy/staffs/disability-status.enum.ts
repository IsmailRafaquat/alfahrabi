import { mapEnumToOptions } from '@abp/ng.core';

export enum DisabilityStatus {
  None = 1,
  Physical = 2,
  Visual = 3,
  Hearing = 4,
  Speech = 5,
  Learning = 6,
  Mental = 7,
  Other = 8,
}

export const disabilityStatusOptions = mapEnumToOptions(DisabilityStatus);
