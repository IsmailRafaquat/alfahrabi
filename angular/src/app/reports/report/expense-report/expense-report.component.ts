import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ExpenseCategoryLookupDto,
  ExpenseCategoryService,
} from 'src/app/proxy/expenses/expense-categories';
import {
  ExpenseReportService,
  ExpenseReportFilterDto,
  ExpenseReportDto,
  ExpenseReportMonthDto,
} from 'src/app/proxy/reports/expense-report';
import {
  normalizeAndValidatePeriod,
  getPeriodDatesFromFilter,
  buildMonthKeys,
  formatMonthLabel,
  createDefaultPeriodFilters,
  toMonthKey,
} from '../report.helper';
import { SharedModule } from 'src/app/shared/shared.module';
import { TopbarLayoutModule } from 'src/app/components/topbar-layout/topbar-layout.module';
import { PageModule } from '@abp/ng.components/page';
import { saveAs } from 'file-saver';
import { ExpenseReportExcelJsExporter } from './expense-report-excel.exporter';
import { getExpenseMaxRows, getExpenseMonthDetails, getExpenseMonthTotal, getExpenseOverallMonthTotal } from './expense-report.helpers';

@Component({
  selector: 'app-expense-report',
  standalone: true,
  imports: [CommonModule, SharedModule, TopbarLayoutModule, PageModule],
  templateUrl: './expense-report.component.html',
  styleUrl: './expense-report.component.scss',
})
export class ExpenseReportComponent implements OnInit {
  private readonly expenseReportService = inject(ExpenseReportService);
  private readonly expenseCategoryService = inject(ExpenseCategoryService);

  filters: ExpenseReportFilterDto = createDefaultPeriodFilters<ExpenseReportFilterDto>({
    expenseCategoryIds: [],
  });

  data: ExpenseReportDto[] = [];
  monthKeys: string[] = [];
  monthLabels: string[] = [];

  expenseCategoryOptions: ExpenseCategoryLookupDto[] = [];

  ngOnInit(): void {
    this.buildExpenseCategoryOptions();
    this.load();
  }

  load(): void {
    normalizeAndValidatePeriod(this.filters as any);

    const { start, end } = getPeriodDatesFromFilter(this.filters as any);
    this.monthKeys = buildMonthKeys(start, end);
    this.monthLabels = this.monthKeys.map(formatMonthLabel);

    this.expenseReportService
      .getList({
        ...this.filters,
      } as any)
      .subscribe({
        next: res => {
          this.data = res ?? [];
        },
        error: () => {
          this.data = [];
        },
      });
  }

  resetFilters(): void {
    this.filters = createDefaultPeriodFilters<ExpenseReportFilterDto>({
      expenseCategoryIds: [],
    });

    this.load();
  }

  private buildExpenseCategoryOptions(): void {
    this.expenseCategoryService.getExpenseCategoryLookup().subscribe({
      next: res => {
        this.expenseCategoryOptions = res ?? [];
      },
      error: () => {
        this.expenseCategoryOptions = [];
      },
    });
  }

  getMonthDetails(item: ExpenseReportDto, monthKey: string) {
    return getExpenseMonthDetails(item, monthKey);
  }

  getMonthTotal(item: ExpenseReportDto, monthKey: string): number {
    return getExpenseMonthTotal(item, monthKey);
  }

  trackByCategory(index: number, item: ExpenseReportDto): string {
    return String(item.expenseCategoryId ?? index);
  }

  private toMonthKey(monthColumn: ExpenseReportMonthDto): string {
    return toMonthKey(monthColumn.month as any);
  }

  getMaxRows(item: ExpenseReportDto): number {
    return getExpenseMaxRows(item, this.monthKeys);
  }

  getMonthDetailAt(item: ExpenseReportDto, monthKey: string, index: number) {
    const details = this.getMonthDetails(item, monthKey);
    return details[index] ?? null;
  }

  getOverallMonthTotal(monthKey: string): number {
    return getExpenseOverallMonthTotal(this.data, monthKey);
  }

  async exportExcel(): Promise<void> {
    const wb = await ExpenseReportExcelJsExporter.buildWorkbook({
      sheetName: 'Expense Report',
      monthKeys: this.monthKeys,
      monthLabels: this.monthLabels,
      data: this.data,
    });

    const bytes = await wb.xlsx.writeBuffer();

    const fileName = `ExpenseReport_${this.filters.periodStart}_${this.filters.periodEnd}.xlsx`;

    saveAs(
      new Blob([bytes], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      }),
      fileName,
    );
  }
}
