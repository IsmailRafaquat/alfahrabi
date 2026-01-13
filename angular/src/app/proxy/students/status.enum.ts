import { mapEnumToOptions } from '@abp/ng.core';

export enum Status {
  Active = 1,
  Alumni = 2,
  Blocked = 3,
  Inactive = 4,
  Suspended = 5,
  Transferred = 6,
  Withdrawn = 7,
  Graduated = 8,
  Pending = 9,
}

export const statusOptions = mapEnumToOptions(Status);
