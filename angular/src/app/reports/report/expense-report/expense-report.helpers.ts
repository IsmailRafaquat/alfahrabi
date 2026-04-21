import {
  ExpenseReportDto,
  ExpenseReportDetailDto,
} from 'src/app/proxy/reports/expense-report';

export function expenseToMonthKey(date: string | Date): string {
  const d = date instanceof Date ? date : new Date(date);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
}

export function getExpenseMonthDetails(
  item: ExpenseReportDto,
  monthKey: string
): ExpenseReportDetailDto[] {
  const month = (item.monthColumns ?? []).find(
    x => expenseToMonthKey(x.month as any) === monthKey
  );

  return month?.details ?? [];
}

export function getExpenseMonthTotal(
  item: ExpenseReportDto,
  monthKey: string
): number {
  return getExpenseMonthDetails(item, monthKey).reduce(
    (sum, x) => sum + (x.amount ?? 0),
    0
  );
}

export function getExpenseMaxRows(
  item: ExpenseReportDto,
  monthKeys: string[]
): number {
  const lengths = monthKeys.map(mk => getExpenseMonthDetails(item, mk).length);
  const max = Math.max(...lengths, 0);
  return max > 0 ? max : 1;
}

export function getExpenseOverallMonthTotal(
  data: ExpenseReportDto[],
  monthKey: string
): number {
  return (data ?? []).reduce(
    (sum, item) => sum + getExpenseMonthTotal(item, monthKey),
    0
  );
}

export interface ExpenseReportExcelExportInput {
  sheetName: string;
  monthKeys: string[];
  monthLabels: string[];
  data: ExpenseReportDto[];
}