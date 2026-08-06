import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopAiMessageRole {
  User = 0,
  Assistant = 1,
  System = 2,
  Tool = 3,
}

export const shopAiMessageRoleOptions = mapEnumToOptions(ShopAiMessageRole);
