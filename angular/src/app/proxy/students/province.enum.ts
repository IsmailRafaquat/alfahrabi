import { mapEnumToOptions } from '@abp/ng.core';

export enum Province {
  Unknown = 0,
  Punjab = 1,
  Sindh = 2,
  KhyberPakhtunkhwa = 3,
  Balochistan = 4,
  IslamabadCapitalTerritory = 5,
  GilgitBaltistan = 6,
  AzadJammuAndKashmir = 7,
}

export const provinceOptions = mapEnumToOptions(Province);
