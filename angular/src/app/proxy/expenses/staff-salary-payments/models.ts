import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateStaffSalaryPaymentDto {
  staffId: string;
  salaryMonth: string;
  salaryAmount: number;
  paymentDate: string;
  remarks?: string;
}

export interface GetStaffSalaryPaymentListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  staffId?: string;
  salaryMonth?: string;
  fromDate?: string;
  toDate?: string;
}

export interface StaffSalaryPaymentDto extends FullAuditedEntityDto<string> {
  tenantId?: string;
  staffId?: string;
  staffName?: string;
  salaryMonth?: string;
  salaryAmount: number;
  paymentDate?: string;
  remarks?: string;
}
