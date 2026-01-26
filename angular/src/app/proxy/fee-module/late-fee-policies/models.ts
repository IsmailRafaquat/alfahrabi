import type { GradeLevel } from '../../students/grade-level.enum';
import type { Section } from '../../students/section.enum';
import type { Shift } from '../../students/shift.enum';
import type { Term } from '../../students/term.enum';
import type { LateFeeType } from './late-fee-type.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateLateFeePolicyDto {
  gradeLevel?: GradeLevel;
  section?: Section;
  shift?: Shift;
  term?: Term;
  graceDays: number;
  type: LateFeeType;
  value: number;
  isActive: boolean;
}

export interface GetLateFeePolicyListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  gradeLevel?: GradeLevel;
  section?: Section;
  shift?: Shift;
  term?: Term;
  type?: LateFeeType;
  isActive?: boolean;
  isGlobal?: boolean;
}

export interface LateFeePolicyDto extends EntityDto<string> {
  gradeLevel?: GradeLevel;
  section?: Section;
  shift?: Shift;
  term?: Term;
  graceDays: number;
  type?: LateFeeType;
  value: number;
  isActive: boolean;
  isGlobal: boolean;
  creationTime?: string;
}
