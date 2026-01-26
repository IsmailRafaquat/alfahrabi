import type { BulkGenerateStudentMonthlyFeeResultDto, CalculateFeeLineAmountsInput, CalculatedAmountsDto, CheckFeesDashboardDto, CheckFeesDashboardInput, CreateUpdateStudentMonthlyFeeLineDto, GetStudentMonthlyFeeLineListInput, StudentMonthlyFeeLineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { BulkGenerateStudentMonthlyFeeDto } from '../student-monthly-fees/models';

@Injectable({
  providedIn: 'root',
})
export class StudentMonthlyFeeLineService {
  apiName = 'Default';
  

  bulkGenerate = (input: BulkGenerateStudentMonthlyFeeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BulkGenerateStudentMonthlyFeeResultDto>({
      method: 'POST',
      url: '/api/fee-module/student-monthly-fee-lines/bulk-generate',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  calculateAmounts = (input: CalculateFeeLineAmountsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CalculatedAmountsDto>({
      method: 'POST',
      url: '/api/fee-module/student-monthly-fee-lines/calculate-amounts',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateStudentMonthlyFeeLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentMonthlyFeeLineDto>({
      method: 'POST',
      url: '/api/fee-module/student-monthly-fee-lines',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/fee-module/student-monthly-fee-lines/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentMonthlyFeeLineDto>({
      method: 'GET',
      url: `/api/fee-module/student-monthly-fee-lines/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getDashboard = (input: CheckFeesDashboardInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CheckFeesDashboardDto>({
      method: 'POST',
      url: '/api/fee-module/student-monthly-fee-lines/get-dashboard',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStudentMonthlyFeeLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentMonthlyFeeLineDto>>({
      method: 'GET',
      url: '/api/fee-module/student-monthly-fee-lines',
      params: { studentMonthlyFeeId: input.studentMonthlyFeeId, feeHeadId: input.feeHeadId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateStudentMonthlyFeeLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/fee-module/student-monthly-fee-lines/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
