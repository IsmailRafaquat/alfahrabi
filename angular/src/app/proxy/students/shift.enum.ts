import { mapEnumToOptions } from '@abp/ng.core';

export enum Shift {
  Morning = 1,
  Evening = 2,
}

export const shiftOptions = mapEnumToOptions(Shift);
