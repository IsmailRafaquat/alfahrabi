import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopNotificationType } from './shop-notification-type.enum';
import type { ShopNotificationSeverity } from './shop-notification-severity.enum';
import type { ShopNotificationStatus } from './shop-notification-status.enum';

export interface GetShopNotificationsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  type?: ShopNotificationType;
  severity?: ShopNotificationSeverity;
  status?: ShopNotificationStatus;
  dateFrom?: string;
  dateTo?: string;
  unreadOnly: boolean;
  referenceType?: string;
}

export interface ShopNotificationDto extends EntityDto<string> {
  type?: ShopNotificationType;
  severity?: ShopNotificationSeverity;
  status?: ShopNotificationStatus;
  title?: string;
  message?: string;
  referenceType?: string;
  referenceId?: string;
  referenceNumber?: string;
  navigationUrl?: string;
  actionLabel?: string;
  triggeredDate?: string;
  readDate?: string;
  expiresDate?: string;
  isUnread: boolean;
}

export interface ShopNotificationSettingsDto {
  notificationsEnabled: boolean;
  lowStockNotificationsEnabled: boolean;
  outOfStockNotificationsEnabled: boolean;
  nearExpiryNotificationsEnabled: boolean;
  expiredBatchNotificationsEnabled: boolean;
  customerDueNotificationsEnabled: boolean;
  supplierDueNotificationsEnabled: boolean;
  cashDifferenceNotificationsEnabled: boolean;
  bankLowBalanceNotificationsEnabled: boolean;
  pendingDraftNotificationsEnabled: boolean;
  profitLossWarningNotificationsEnabled: boolean;
  nearExpiryDefaultDays: number;
  customerDueReminderDays: number;
  supplierDueReminderDays: number;
  draftPendingHours: number;
  bankLowBalanceThreshold: number;
  profitLossWarningThreshold: number;
  notificationRetentionDays: number;
  emailNotificationsEnabled: boolean;
}

export interface ShopNotificationSummaryDto {
  totalUnread: number;
  informationCount: number;
  warningCount: number;
  criticalCount: number;
  todayCount: number;
  lowStockCount: number;
  expiryCount: number;
  paymentDueCount: number;
  draftPendingCount: number;
}

export interface UpdateShopNotificationSettingsDto {
  notificationsEnabled: boolean;
  lowStockNotificationsEnabled: boolean;
  outOfStockNotificationsEnabled: boolean;
  nearExpiryNotificationsEnabled: boolean;
  expiredBatchNotificationsEnabled: boolean;
  customerDueNotificationsEnabled: boolean;
  supplierDueNotificationsEnabled: boolean;
  cashDifferenceNotificationsEnabled: boolean;
  bankLowBalanceNotificationsEnabled: boolean;
  pendingDraftNotificationsEnabled: boolean;
  profitLossWarningNotificationsEnabled: boolean;
  nearExpiryDefaultDays: number;
  customerDueReminderDays: number;
  supplierDueReminderDays: number;
  draftPendingHours: number;
  bankLowBalanceThreshold: number;
  profitLossWarningThreshold: number;
  notificationRetentionDays: number;
  emailNotificationsEnabled: boolean;
}
