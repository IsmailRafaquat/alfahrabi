import type { CreateUpdateShopBankAccountDto, GetShopBankAccountsInput, ShopBankAccountDto, ShopBankAccountLookupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopBankAccountService {
  apiName = 'Default';
  

  create = (input: CreateUpdateShopBankAccountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankAccountDto>({
      method: 'POST',
      url: '/api/app/shop-bank-account',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-bank-account/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankAccountDto>({
      method: 'GET',
      url: `/api/app/shop-bank-account/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopBankAccountsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopBankAccountDto>>({
      method: 'GET',
      url: '/api/app/shop-bank-account',
      params: { filter: input.filter, isActive: input.isActive, isDefault: input.isDefault, bankName: input.bankName, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopBankAccountLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-bank-account/lookup',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopBankAccountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankAccountDto>({
      method: 'PUT',
      url: `/api/app/shop-bank-account/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
