import type { DiscountType } from './discount-type.enum';
import type { GradeLevel } from '../../students/grade-level.enum';
import type { Section } from '../../students/section.enum';
import type { Shift } from '../../students/shift.enum';
import type { Term } from '../../students/term.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface BulkAssignStudentFeeDiscountDto {
  feeHeadId?: string;
  discountType: DiscountType;
  value: number;
  startMonth?: number;
  endMonth?: number;
  reason: string;
  approvedByStaffId?: string;
  isActive: boolean;
  gradeLevel?: GradeLevel;
  section?: Section;
  shift?: Shift;
  term?: Term;
  skipExisting: boolean;
}

export interface BulkAssignStudentFeeDiscountResultDto {
  totalStudents: number;
  created: number;
  skippedExisting: number;
  skippedStudentNames: string[];
}

export interface CreateUpdateStudentFeeDiscountDto {
  studentId: string;
  feeHeadId?: string;
  discountType: DiscountType;
  value: number;
  startMonth?: number;
  endMonth?: number;
  reason: string;
  approvedByStaffId?: string;
  isActive: boolean;
}

export interface GetStudentFeeDiscountListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  studentId?: string;
  feeHeadId?: string;
  discountType?: DiscountType;
  isActive?: boolean;
  isApproved?: boolean;
}

export interface StudentFeeDiscountDto extends EntityDto<string> {
  studentId?: string;
  studentName?: string;
  feeHeadId?: string;
  feeHeadName?: string;
  discountType?: DiscountType;
  value: number;
  startMonth?: number;
  endMonth?: number;
  reason?: string;
  approvedByStaffId?: string;
  approvedByStaffName?: string;
  isActive: boolean;
}
