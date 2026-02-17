import type { CreateUpdateExpenseCategoryDto, ExpenseCategoryDto, ExpenseCategoryLookupDto, GetExpenseCategoryListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExpenseCategoryService {
  apiName = 'Default';
  

  create = (input: CreateUpdateExpenseCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExpenseCategoryDto>({
      method: 'POST',
      url: '/api/app/expense-categories',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/expense-categories/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExpenseCategoryDto>({
      method: 'GET',
      url: `/api/app/expense-categories/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getExpenseCategoryLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExpenseCategoryLookupDto[]>({
      method: 'GET',
      url: '/api/app/expense-categories/get-expense-category-lookup',
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetExpenseCategoryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ExpenseCategoryDto>>({
      method: 'GET',
      url: '/api/app/expense-categories',
      params: { filter: input.filter, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  setActive = (id: string, isActive: boolean, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/app/expense-categories/${id}/active`,
      body: isActive,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateExpenseCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExpenseCategoryDto>({
      method: 'PUT',
      url: `/api/app/expense-categories/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
