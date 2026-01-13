import { mapEnumToOptions } from '@abp/ng.core';

export enum EmploymentType {
  FullTime = 1,
  PartTime = 2,
  Contract = 3,
  Temporary = 4,
  Internship = 5,
}

export const employmentTypeOptions = mapEnumToOptions(EmploymentType);
