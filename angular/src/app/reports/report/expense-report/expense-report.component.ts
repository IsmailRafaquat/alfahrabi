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
} from '../report.helper';
import { SharedModule } from 'src/app/shared/shared.module';
import { TopbarLayoutModule } from 'src/app/components/topbar-layout/topbar-layout.module';
import { PageModule } from '@abp/ng.components/page';
import { saveAs } from 'file-saver';
import { ExpenseReportExcelJsExporter } from './expense-report-excel.exporter';

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

  filters: ExpenseReportFilterDto = {
    filter: '',
    periodStart: undefined,
    periodEnd: undefined,
    expenseCategoryIds: [],
  };

  data: ExpenseReportDto[] = [];
  monthKeys: string[] = [];
  monthLabels: string[] = [];

  expenseCategoryOptions: ExpenseCategoryLookupDto[] = [];

  ngOnInit(): void {
    const year = new Date().getFullYear();

    this.filters.periodStart = `${year}-01` as any;
    this.filters.periodEnd = `${year}-12` as any;

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
    const year = new Date().getFullYear();

    this.filters = {
      filter: '',
      periodStart: `${year}-01` as any,
      periodEnd: `${year}-12` as any,
      expenseCategoryIds: [],
    };

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
    const month = item.monthColumns?.find(x => this.toMonthKey(x) === monthKey);
    return month?.details ?? [];
  }

  getMonthTotal(item: ExpenseReportDto, monthKey: string): number {
    const details = this.getMonthDetails(item, monthKey);
    return details.reduce((sum, x) => sum + (x.amount ?? 0), 0);
  }

  trackByCategory(index: number, item: ExpenseReportDto): string {
    return String(item.expenseCategoryId ?? index);
  }

  private toMonthKey(monthColumn: ExpenseReportMonthDto): string {
    const date = new Date(monthColumn.month as any);
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    return `${year}-${month}`;
  }

  getMaxRows(item: ExpenseReportDto): number {
    if (!item.monthColumns?.length) return 1;

    const lengths = this.monthKeys.map(mk => this.getMonthDetails(item, mk).length);
    const max = Math.max(...lengths, 0);

    return max > 0 ? max : 1;
  }

  getMonthDetailAt(item: ExpenseReportDto, monthKey: string, index: number) {
    const details = this.getMonthDetails(item, monthKey);
    return details[index] ?? null;
  }

  getOverallMonthTotal(monthKey: string): number {
    return this.data.reduce((sum, item) => sum + this.getMonthTotal(item, monthKey), 0);
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
