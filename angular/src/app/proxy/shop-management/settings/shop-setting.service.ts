import type { CreateUpdateShopSettingDto, ShopSettingDto, ShopSettingSetupStatusDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopSettingService {
  apiName = 'Default';
  

  createOrUpdate = (input: CreateUpdateShopSettingDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSettingDto>({
      method: 'POST',
      url: '/api/app/shop-setting/or-update',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSettingDto>({
      method: 'GET',
      url: '/api/app/shop-setting',
    },
    { apiName: this.apiName,...config });
  

  getSetupStatus = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSettingSetupStatusDto>({
      method: 'GET',
      url: '/api/app/shop-setting/setup-status',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
