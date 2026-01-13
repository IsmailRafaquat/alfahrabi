import { mapEnumToOptions } from '@abp/ng.core';

export enum GradeLevel {
  Unknown = 0,
  Grade1 = 1,
  Grade2 = 2,
  Grade3 = 3,
  Grade4 = 4,
  Grade5 = 5,
  Grade6 = 6,
  Grade7 = 7,
  Grade8 = 8,
  Grade9 = 9,
  Grade10 = 10,
  Grade11 = 11,
  Grade12 = 12,
  PreNursery = 13,
  PlayGroup = 14,
  Nursery = 15,
  KG1 = 16,
  LKG = 17,
  KG2 = 18,
  UKG = 19,
  Reception = 20,
  Prep = 21,
}

export const gradeLevelOptions = mapEnumToOptions(GradeLevel);
