import { mapEnumToOptions } from '@abp/ng.core';

export enum Term {
  Unknown = 0,
  Fall = 14,
  Spring = 15,
  Summer = 16,
  Term1 = 17,
  Term2 = 18,
  Term3 = 19,
}

export const termOptions = mapEnumToOptions(Term);
