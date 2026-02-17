import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateExpenseEntryDto {
  expenseDate: string;
  expenseCategoryId: string;
  title: string;
  amount: number;
  paidTo?: string;
  remarks?: string;
}

export interface ExpenseEntryDto extends FullAuditedEntityDto<string> {
  tenantId?: string;
  expenseDate?: string;
  expenseCategoryId?: string;
  expenseCategoryName?: string;
  title?: string;
  amount: number;
  paidTo?: string;
  remarks?: string;
}

export interface GetExpenseEntryListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  expenseCategoryId?: string;
  fromDate?: string;
  toDate?: string;
}
