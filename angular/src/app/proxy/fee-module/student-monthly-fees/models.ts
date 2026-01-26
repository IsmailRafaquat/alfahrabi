import type { GradeLevel } from '../../students/grade-level.enum';
import type { Section } from '../../students/section.enum';
import type { Shift } from '../../students/shift.enum';
import type { Term } from '../../students/term.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface BulkGenerateStudentMonthlyFeeDto {
  gradeLevel?: GradeLevel;
  section?: Section;
  shift?: Shift;
  term?: Term;
  month: string;
  dueDate?: string;
  remarks?: string;
  skipIfExists: boolean;
}

export interface BulkGenerateStudentMonthlyFeeResultDto {
  totalStudents: number;
  created: number;
  skipped: number;
}

export interface CreateUpdateStudentMonthlyFeeDto {
  studentId: string;
  month: string;
  dueDate?: string;
  remarks?: string;
}

export interface GetStudentMonthlyFeeListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  studentId?: string;
  month?: string;
}

export interface StudentMonthlyFeeDto extends EntityDto<string> {
  tenantId?: string;
  studentId?: string;
  month?: string;
  dueDate?: string;
  remarks?: string;
  studentName?: string;
  admissionNo?: string;
}
