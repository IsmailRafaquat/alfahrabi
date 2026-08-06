import type { CancelShopExpenseDto, CreateUpdateShopExpenseDto, GetShopExpensesInput, ShopExpenseDto, ShopExpenseSummaryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopExpenseService {
  apiName = 'Default';
  

  cancel = (id: string, input: CancelShopExpenseDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopExpenseDto>({
      method: 'POST',
      url: `/api/app/shop-expense/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateShopExpenseDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopExpenseDto>({
      method: 'POST',
      url: '/api/app/shop-expense',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-expense/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopExpenseDto>({
      method: 'GET',
      url: `/api/app/shop-expense/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopExpensesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopExpenseDto>>({
      method: 'GET',
      url: '/api/app/shop-expense',
      params: { filter: input.filter, expenseCategoryId: input.expenseCategoryId, status: input.status, paymentMethod: input.paymentMethod, expenseDateFrom: input.expenseDateFrom, expenseDateTo: input.expenseDateTo, minimumAmount: input.minimumAmount, maximumAmount: input.maximumAmount, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSummary = (input: GetShopExpensesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopExpenseSummaryDto>({
      method: 'GET',
      url: '/api/app/shop-expense/summary',
      params: { filter: input.filter, expenseCategoryId: input.expenseCategoryId, status: input.status, paymentMethod: input.paymentMethod, expenseDateFrom: input.expenseDateFrom, expenseDateTo: input.expenseDateTo, minimumAmount: input.minimumAmount, maximumAmount: input.maximumAmount, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  post = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopExpenseDto>({
      method: 'POST',
      url: `/api/app/shop-expense/${id}`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopExpenseDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopExpenseDto>({
      method: 'PUT',
      url: `/api/app/shop-expense/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
