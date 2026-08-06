import type { CreateUpdateShopUnitDto, GetShopUnitsInput, ShopUnitDto, ShopUnitLookupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopUnitService {
  apiName = 'Default';
  

  create = (input: CreateUpdateShopUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopUnitDto>({
      method: 'POST',
      url: '/api/app/shop-unit',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-unit/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopUnitDto>({
      method: 'GET',
      url: `/api/app/shop-unit/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopUnitsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopUnitDto>>({
      method: 'GET',
      url: '/api/app/shop-unit',
      params: { filter: input.filter, allowDecimal: input.allowDecimal, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopUnitLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-unit/lookup',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopUnitDto>({
      method: 'PUT',
      url: `/api/app/shop-unit/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
