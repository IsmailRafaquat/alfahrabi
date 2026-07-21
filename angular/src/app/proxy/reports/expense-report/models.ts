
export interface ExpenseReportDetailDto {
  expenseEntryId?: string;
  expenseDate?: string;
  title?: string;
  amount: number;
  paidTo?: string;
  remarks?: string;
}

export interface ExpenseReportDto {
  expenseCategoryId?: string;
  expenseCategoryName?: string;
  monthColumns: ExpenseReportMonthDto[];
}

export interface ExpenseReportFilterDto {
  filter?: string;
  periodStart?: string;
  periodEnd?: string;
  expenseCategoryIds: string[];
}

export interface ExpenseReportMonthDto {
  month?: string;
  details: ExpenseReportDetailDto[];
}
