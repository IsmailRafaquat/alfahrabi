import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface BulkGenerateStudentMonthlyFeeResultDto {
  totalStudents: number;
  created: number;
  skipped: number;
  linesCreated: number;
  linesSkipped: number;
  missingFeeProfile: number;
}

export interface CalculateFeeLineAmountsInput {
  studentMonthlyFeeId?: string;
  feeHeadId?: string;
}

export interface CalculatedAmountsDto {
  expectedAmount: number;
  discountAmount: number;
  lateFeeAmount: number;
  netAmount: number;
}

export interface CheckFeesDashboardDto {
  month?: string;
  totalExpected: number;
  totalDiscount: number;
  totalLateFee: number;
  totalNet: number;
  totalPaid: number;
  totalPending: number;
  byFeeHead: FeeHeadSummaryDto[];
  byStudent: StudentFeeSummaryDto[];
  totalStudents: number;
}

export interface CheckFeesDashboardInput extends PagedAndSortedResultRequestDto {
  month?: string;
  asOfDate?: string;
  gradeLevel?: number;
  section?: number;
  shift?: number;
  term?: number;
  studentId?: string;
}

export interface CreateUpdateStudentMonthlyFeeLineDto {
  studentMonthlyFeeId: string;
  feeHeadId: string;
  expectedAmount: number;
  discountAmount: number;
  adjustmentAmount: number;
  lateFeeAmount: number;
  paidAmount: number;
}

export interface FeeHeadSummaryDto {
  feeHeadId?: string;
  feeHeadName?: string;
  expected: number;
  discount: number;
  lateFee: number;
  net: number;
  paid: number;
  pending: number;
}

export interface GetStudentMonthlyFeeLineListInput extends PagedAndSortedResultRequestDto {
  studentMonthlyFeeId?: string;
  feeHeadId?: string;
}

export interface StudentFeeSummaryDto {
  studentId?: string;
  studentName?: string;
  net: number;
  paid: number;
  pending: number;
}

export interface StudentMonthlyFeeLineDto extends EntityDto<string> {
  tenantId?: string;
  studentMonthlyFeeId?: string;
  feeHeadId?: string;
  expectedAmount: number;
  discountAmount: number;
  adjustmentAmount: number;
  lateFeeAmount: number;
  paidAmount: number;
  netAmount: number;
  outstandingAmount: number;
  feeHeadName?: string;
}
