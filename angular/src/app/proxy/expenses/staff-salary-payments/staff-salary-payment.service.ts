import type { CreateUpdateStaffSalaryPaymentDto, GetStaffSalaryPaymentListInput, StaffSalaryPaymentDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StaffSalaryPaymentService {
  apiName = 'Default';
  

  create = (input: CreateUpdateStaffSalaryPaymentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffSalaryPaymentDto>({
      method: 'POST',
      url: '/api/app/staff-salary-payments',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/staff-salary-payments/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffSalaryPaymentDto>({
      method: 'GET',
      url: `/api/app/staff-salary-payments/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStaffSalaryPaymentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StaffSalaryPaymentDto>>({
      method: 'GET',
      url: '/api/app/staff-salary-payments',
      params: { filter: input.filter, staffId: input.staffId, salaryMonth: input.salaryMonth, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateStaffSalaryPaymentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffSalaryPaymentDto>({
      method: 'PUT',
      url: `/api/app/staff-salary-payments/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
