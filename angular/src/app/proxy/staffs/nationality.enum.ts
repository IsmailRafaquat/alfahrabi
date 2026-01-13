import { mapEnumToOptions } from '@abp/ng.core';

export enum Nationality {
  Pakistani = 1,
  Indian = 2,
  Bangladeshi = 3,
  Afghan = 4,
  Chinese = 5,
  American = 6,
  British = 7,
  Canadian = 8,
  Other = 9,
}

export const nationalityOptions = mapEnumToOptions(Nationality);
