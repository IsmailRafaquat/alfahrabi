import type { PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface ExpensesByCategoryDto {
  expenseCategoryId?: string;
  expenseCategoryName?: string;
  amount: number;
  count: number;
}

export interface ExpensesByDayDto {
  date?: string;
  amount: number;
  count: number;
}

export interface ExpensesDashboardDto {
  totalExpenses: number;
  totalOtherExpenses: number;
  totalSalaryPayments: number;
  totalTransactions: number;
  byCategory: ExpensesByCategoryDto[];
  byDay: ExpensesByDayDto[];
  recentExpenses: RecentExpenseDto[];
}

export interface GetExpensesDashboardInput extends PagedAndSortedResultRequestDto {
  month?: string;
  fromDate?: string;
  toDate?: string;
  expenseCategoryId?: string;
  filter?: string;
}

export interface RecentExpenseDto {
  date?: string;
  source?: string;
  title?: string;
  categoryOrStaff?: string;
  amount: number;
}
