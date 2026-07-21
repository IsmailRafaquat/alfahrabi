import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { StaffSalaryReportDto, StaffSalaryReportFilterDto } from '../salary-report/models';

@Injectable({
  providedIn: 'root',
})
export class StaffSalaryReportService {
  apiName = 'Default';
  

  getList = (input: StaffSalaryReportFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffSalaryReportDto>({
      method: 'GET',
      url: '/api/app/staff-report',
      params: { periodStart: input.periodStart, periodEnd: input.periodEnd, filter: input.filter, staffIds: input.staffIds },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
