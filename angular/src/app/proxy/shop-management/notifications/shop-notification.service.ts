import type { GetShopNotificationsInput, ShopNotificationDto, ShopNotificationSettingsDto, ShopNotificationSummaryDto, UpdateShopNotificationSettingsDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopNotificationService {
  apiName = 'Default';
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-notification/${id}`,
    },
    { apiName: this.apiName,...config });
  

  dismiss = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/shop-notification/${id}/dismiss`,
    },
    { apiName: this.apiName,...config });
  

  dismissAll = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/shop-notification/dismiss-all',
    },
    { apiName: this.apiName,...config });
  

  generateNow = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/shop-notification/generate-now',
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopNotificationDto>({
      method: 'GET',
      url: `/api/app/shop-notification/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopNotificationsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopNotificationDto>>({
      method: 'GET',
      url: '/api/app/shop-notification',
      params: { filter: input.filter, type: input.type, severity: input.severity, status: input.status, dateFrom: input.dateFrom, dateTo: input.dateTo, unreadOnly: input.unreadOnly, referenceType: input.referenceType, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSettings = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopNotificationSettingsDto>({
      method: 'GET',
      url: '/api/app/shop-notification/settings',
    },
    { apiName: this.apiName,...config });
  

  getSummary = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopNotificationSummaryDto>({
      method: 'GET',
      url: '/api/app/shop-notification/summary',
    },
    { apiName: this.apiName,...config });
  

  markAllAsRead = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/shop-notification/mark-all-as-read',
    },
    { apiName: this.apiName,...config });
  

  markAsRead = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/shop-notification/${id}/mark-as-read`,
    },
    { apiName: this.apiName,...config });
  

  markAsUnread = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/shop-notification/${id}/mark-as-unread`,
    },
    { apiName: this.apiName,...config });
  

  updateSettings = (input: UpdateShopNotificationSettingsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/shop-notification/settings',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
