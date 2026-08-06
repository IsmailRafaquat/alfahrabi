import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopNotificationStatus {
  Unread = 0,
  Read = 1,
  Dismissed = 2,
  Resolved = 3,
}

export const shopNotificationStatusOptions = mapEnumToOptions(ShopNotificationStatus);
