import { mapEnumToOptions } from '@abp/ng.core';

export enum RelationShipToStudent {
  Unknown = 0,
  Father = 1,
  Mother = 2,
  Guardian = 3,
  StepFather = 4,
  StepMother = 5,
  Brother = 6,
  Sister = 7,
  Uncle = 8,
  Aunt = 9,
  Grandfather = 10,
  Grandmother = 11,
  Self = 12,
  Other = 13,
}

export const relationShipToStudentOptions = mapEnumToOptions(RelationShipToStudent);
