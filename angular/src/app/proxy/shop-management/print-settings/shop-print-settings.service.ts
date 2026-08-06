import type { CreateUpdateShopPrintSettingsDto, ShopPrintSettingsDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopPrintSettingsService {
  apiName = 'Default';
  

  createOrUpdate = (input: CreateUpdateShopPrintSettingsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPrintSettingsDto>({
      method: 'POST',
      url: '/api/app/shop-print-settings/or-update',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPrintSettingsDto>({
      method: 'GET',
      url: '/api/app/shop-print-settings',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
