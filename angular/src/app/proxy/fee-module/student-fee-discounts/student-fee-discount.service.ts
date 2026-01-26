import type { BulkAssignStudentFeeDiscountDto, BulkAssignStudentFeeDiscountResultDto, CreateUpdateStudentFeeDiscountDto, GetStudentFeeDiscountListInput, StudentFeeDiscountDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentFeeDiscountService {
  apiName = 'Default';
  

  bulkAssign = (input: BulkAssignStudentFeeDiscountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BulkAssignStudentFeeDiscountResultDto>({
      method: 'POST',
      url: '/api/fee-module/student-fee-discounts/bulk-assign',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateStudentFeeDiscountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentFeeDiscountDto>({
      method: 'POST',
      url: '/api/fee-module/student-fee-discounts',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/fee-module/student-fee-discounts/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentFeeDiscountDto>({
      method: 'GET',
      url: `/api/fee-module/student-fee-discounts/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStudentFeeDiscountListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentFeeDiscountDto>>({
      method: 'GET',
      url: '/api/fee-module/student-fee-discounts',
      params: { filter: input.filter, studentId: input.studentId, feeHeadId: input.feeHeadId, discountType: input.discountType, isActive: input.isActive, isApproved: input.isApproved, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateStudentFeeDiscountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/fee-module/student-fee-discounts/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
