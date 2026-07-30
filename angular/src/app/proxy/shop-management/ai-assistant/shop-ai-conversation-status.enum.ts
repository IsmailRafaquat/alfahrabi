import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopAiConversationStatus {
  Active = 0,
  Archived = 1,
}

export const shopAiConversationStatusOptions = mapEnumToOptions(ShopAiConversationStatus);
