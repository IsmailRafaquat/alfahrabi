import type { GradeLevel } from '../students/grade-level.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateSubjectDto {
  code?: string;
  name: string;
  shortName?: string;
  description?: string;
  gradeLevel?: GradeLevel;
  creditHours?: number;
  isActive: boolean;
}

export interface GetSubjectListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  code?: string;
  name?: string;
  gradeLevel?: GradeLevel;
  isActive?: boolean;
}

export interface SubjectDto extends EntityDto<string> {
  tenantId?: string;
  code?: string;
  name?: string;
  shortName?: string;
  description?: string;
  gradeLevel?: GradeLevel;
  creditHours?: number;
  isActive: boolean;
}

export interface UpdateSubjectDto {
  code: string;
  name: string;
  shortName?: string;
  description?: string;
  gradeLevel?: GradeLevel;
  creditHours?: number;
  isActive: boolean;
}
