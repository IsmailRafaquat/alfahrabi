import type { ExpenseReportDto, ExpenseReportFilterDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExpenseReportService {
  apiName = 'Default';
  

  getList = (input: ExpenseReportFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExpenseReportDto[]>({
      method: 'GET',
      url: '/api/app/expense-report',
      params: { filter: input.filter, periodStart: input.periodStart, periodEnd: input.periodEnd, expenseCategoryIds: input.expenseCategoryIds },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
