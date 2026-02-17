import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateExpenseCategoryDto {
  name: string;
  isActive: boolean;
}

export interface ExpenseCategoryDto extends FullAuditedEntityDto<string> {
  tenantId?: string;
  name?: string;
  isActive: boolean;
}

export interface ExpenseCategoryLookupDto {
  id?: string;
  name?: string;
}

export interface GetExpenseCategoryListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  isActive?: boolean;
}
