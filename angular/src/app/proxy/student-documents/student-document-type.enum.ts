import { mapEnumToOptions } from '@abp/ng.core';

export enum StudentDocumentType {
  NationalId = 1,
  BForm = 2,
  Passport = 3,
  BirthCertificate = 4,
  StudentCard = 5,
  Transcript = 6,
  Certificate = 7,
  Other = 8,
}

export const studentDocumentTypeOptions = mapEnumToOptions(StudentDocumentType);
