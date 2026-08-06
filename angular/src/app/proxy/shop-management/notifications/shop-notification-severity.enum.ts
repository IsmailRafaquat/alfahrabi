import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopNotificationSeverity {
  Information = 0,
  Success = 1,
  Warning = 2,
  Critical = 3,
}

export const shopNotificationSeverityOptions = mapEnumToOptions(ShopNotificationSeverity);
