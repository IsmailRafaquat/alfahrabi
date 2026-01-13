import { mapEnumToOptions } from '@abp/ng.core';

export enum StaffDocumentType {
  NationalId = 1,
  Passport = 2,
  Resume = 3,
  Degree = 4,
  Certificate = 5,
  Transcript = 6,
  ExperienceLetter = 7,
  Contract = 8,
  Other = 9,
}

export const staffDocumentTypeOptions = mapEnumToOptions(StaffDocumentType);
