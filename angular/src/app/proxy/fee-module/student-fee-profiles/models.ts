import type { GradeLevel } from '../../students/grade-level.enum';
import type { Section } from '../../students/section.enum';
import type { Shift } from '../../students/shift.enum';
import type { Term } from '../../students/term.enum';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface BulkAssignStudentFeeProfileDto {
  feeStructureId: string;
  gradeLevel: GradeLevel;
  section?: Section;
  shift?: Shift;
  term?: Term;
  effectiveFrom: string;
  effectiveTo?: string;
  isActive: boolean;
  skipExisting: boolean;
}

export interface BulkAssignStudentFeeProfileResultDto {
  totalStudents: number;
  created: number;
  skippedExisting: number;
  skippedStudentNames: string[];
}

export interface CreateUpdateStudentFeeProfileDto {
  studentId: string;
  feeStructureId: string;
  effectiveFrom: string;
  effectiveTo?: string;
  isActive: boolean;
}

export interface GetStudentFeeProfileListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  studentId?: string;
  feeStructureId?: string;
  isActive?: boolean;
  effectiveFrom?: string;
  effectiveTo?: string;
}

export interface StudentFeeProfileDto extends FullAuditedEntityDto<string> {
  tenantId?: string;
  studentId?: string;
  feeStructureId?: string;
  effectiveFrom?: string;
  effectiveTo?: string;
  isActive: boolean;
}
