import type { BulkGenerateStudentMonthlyFeeDto, BulkGenerateStudentMonthlyFeeResultDto, CreateUpdateStudentMonthlyFeeDto, GetStudentMonthlyFeeListInput, StudentMonthlyFeeDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentMonthlyFeeService {
  apiName = 'Default';
  

  bulkGenerate = (input: BulkGenerateStudentMonthlyFeeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BulkGenerateStudentMonthlyFeeResultDto>({
      method: 'POST',
      url: '/api/fee-module/student-monthly-fees/bulk-generate',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateStudentMonthlyFeeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentMonthlyFeeDto>({
      method: 'POST',
      url: '/api/fee-module/student-monthly-fees',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/fee-module/student-monthly-fees/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentMonthlyFeeDto>({
      method: 'GET',
      url: `/api/fee-module/student-monthly-fees/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStudentMonthlyFeeListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentMonthlyFeeDto>>({
      method: 'GET',
      url: '/api/fee-module/student-monthly-fees',
      params: { filter: input.filter, studentId: input.studentId, month: input.month, collectedOn: input.collectedOn, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateStudentMonthlyFeeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/fee-module/student-monthly-fees/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
