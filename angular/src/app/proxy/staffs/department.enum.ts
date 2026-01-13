import { mapEnumToOptions } from '@abp/ng.core';

export enum Department {
  Administration = 1,
  Teaching = 2,
  Finance = 3,
  HR = 4,
  IT = 5,
  Maintenance = 6,
  Transport = 7,
  Security = 8,
  Other = 9,
}

export const departmentOptions = mapEnumToOptions(Department);
