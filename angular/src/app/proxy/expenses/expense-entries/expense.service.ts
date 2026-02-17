import type { CreateUpdateExpenseEntryDto, ExpenseEntryDto, GetExpenseEntryListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExpenseService {
  apiName = 'Default';
  

  create = (input: CreateUpdateExpenseEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExpenseEntryDto>({
      method: 'POST',
      url: '/api/app/expense-entry',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/expense-entry/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExpenseEntryDto>({
      method: 'GET',
      url: `/api/app/expense-entry/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetExpenseEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ExpenseEntryDto>>({
      method: 'GET',
      url: '/api/app/expense-entry',
      params: { filter: input.filter, expenseCategoryId: input.expenseCategoryId, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateExpenseEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExpenseEntryDto>({
      method: 'PUT',
      url: `/api/app/expense-entry/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
