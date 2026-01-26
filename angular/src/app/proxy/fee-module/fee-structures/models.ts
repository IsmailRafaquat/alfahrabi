import type { GradeLevel } from '../../students/grade-level.enum';
import type { Shift } from '../../students/shift.enum';
import type { Term } from '../../students/term.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateFeeStructureDto {
  gradeLevel: GradeLevel;
  shift: Shift;
  term: Term;
  effectiveFrom: string;
  effectiveTo?: string;
  isActive: boolean;
}

export interface FeeStructureDto extends FullAuditedEntityDto<string> {
  tenantId?: string;
  gradeLevel?: GradeLevel;
  shift?: Shift;
  term?: Term;
  effectiveFrom?: string;
  effectiveTo?: string;
  isActive: boolean;
}

export interface FeeStructureLookupDto extends EntityDto<string> {
  displayName?: string;
}

export interface GetFeeStructureListInput extends PagedAndSortedResultRequestDto {
  gradeLevel?: GradeLevel;
  shift?: Shift;
  term?: Term;
  isActive?: boolean;
}
