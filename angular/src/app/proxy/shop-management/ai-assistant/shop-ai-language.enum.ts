import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopAiLanguage {
  Unknown = 0,
  English = 1,
  Urdu = 2,
  RomanUrdu = 3,
  Mixed = 4,
}

export const shopAiLanguageOptions = mapEnumToOptions(ShopAiLanguage);
