import type { BulkAssignStudentFeeProfileDto, BulkAssignStudentFeeProfileResultDto, CreateUpdateStudentFeeProfileDto, GetStudentFeeProfileListInput, StudentFeeProfileDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentFeeProfileService {
  apiName = 'Default';
  

  bulkAssign = (input: BulkAssignStudentFeeProfileDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BulkAssignStudentFeeProfileResultDto>({
      method: 'POST',
      url: '/api/fee-module/student-fee-profiles/bulk-assign',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateStudentFeeProfileDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentFeeProfileDto>({
      method: 'POST',
      url: '/api/fee-module/student-fee-profiles',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/fee-module/student-fee-profiles/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentFeeProfileDto>({
      method: 'GET',
      url: `/api/fee-module/student-fee-profiles/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStudentFeeProfileListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentFeeProfileDto>>({
      method: 'GET',
      url: '/api/fee-module/student-fee-profiles',
      params: { filter: input.filter, studentId: input.studentId, feeStructureId: input.feeStructureId, isActive: input.isActive, effectiveFrom: input.effectiveFrom, effectiveTo: input.effectiveTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateStudentFeeProfileDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/fee-module/student-fee-profiles/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
