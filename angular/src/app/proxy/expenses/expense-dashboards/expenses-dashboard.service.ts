import type { ExpensesDashboardDto, GetExpensesDashboardInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExpensesDashboardService {
  apiName = 'Default';
  

  get = (input: GetExpensesDashboardInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExpensesDashboardDto>({
      method: 'GET',
      url: '/api/app/expenses-dashboard',
      params: { month: input.month, fromDate: input.fromDate, toDate: input.toDate, expenseCategoryId: input.expenseCategoryId, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
