import { mapEnumToOptions } from '@abp/ng.core';

export enum JobStatus {
  Active = 1,
  OnLeave = 2,
  Resigned = 3,
  Terminated = 4,
  Retired = 5,
}

export const jobStatusOptions = mapEnumToOptions(JobStatus);
