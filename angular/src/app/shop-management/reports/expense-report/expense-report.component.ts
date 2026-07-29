import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { ShopExpenseReportGroupBy } from '../../../proxy/shop-management/reports/shop-expense-report-group-by.enum';
import { GetShopExpenseReportInput, ShopExpenseReportResultDto } from '../../../proxy/shop-management/reports/models';
import { ShopExpenseCategoryLookupDto, ShopExpenseCategoryService } from '../../../proxy/shop-management/expense-categories';
import { ShopExpensePaymentMethod } from '../../../proxy/shop-management/expenses/shop-expense-payment-method.enum';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-expense-report',
  standalone: false,
  templateUrl: './expense-report.component.html',
  styleUrl: './expense-report.component.scss',
})
export class ExpenseReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly categoryService = inject(ShopExpenseCategoryService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.Reports.Export');
  readonly ShopExpenseReportGroupBy = ShopExpenseReportGroupBy;

  categories: ShopExpenseCategoryLookupDto[] = [];

  input: GetShopExpenseReportInput = {
    period: ShopReportPeriod.ThisMonth,
    groupBy: ShopExpenseReportGroupBy.None,
    sorting: 'ExpenseDate desc',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopExpenseReportResultDto;
  loading = false;
  exporting = false;
  pageIndex = 1;

  ngOnInit(): void {
    this.categoryService.getLookup().subscribe(r => (this.categories = r.items || []));
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .getExpenseReport(this.input)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: r => (this.result = r),
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  onFilterChange(): void {
    this.input.skipCount = 0;
    this.pageIndex = 1;
    this.load();
  }

  reset(): void {
    this.input = {
      period: ShopReportPeriod.ThisMonth,
      groupBy: ShopExpenseReportGroupBy.None,
      sorting: 'ExpenseDate desc',
      skipCount: 0,
      maxResultCount: 20,
    };
    this.pageIndex = 1;
    this.load();
  }

  onPageChange(page: number): void {
    this.pageIndex = page;
    this.input.skipCount = (page - 1) * (this.input.maxResultCount || 20);
    this.load();
  }

  export(format: ShopReportExportFormat): void {
    this.exporting = true;
    this.service
      .exportExpenseReport(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `expense-report.${this.extensionFor(format)}`),
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  private extensionFor(format: ShopReportExportFormat): string {
    switch (format) {
      case ShopReportExportFormat.Excel: return 'xlsx';
      case ShopReportExportFormat.Csv: return 'csv';
      default: return 'html';
    }
  }

  get totalsCards(): ReportTotalCard[] {
    const t = this.result?.totals;
    if (!t) return [];
    return [
      { label: 'Expense Count', value: String(t.expenseCount), icon: 'fas fa-file-invoice', colorClass: 'primary' },
      { label: 'Total Amount', value: t.totalExpenseAmount.toFixed(2), icon: 'fas fa-coins', colorClass: 'danger' },
      { label: 'Cash', value: t.cashExpenses.toFixed(2), icon: 'fas fa-money-bill-wave', colorClass: 'success' },
      { label: 'Bank', value: t.bankExpenses.toFixed(2), icon: 'fas fa-university', colorClass: 'info' },
      { label: 'Average Expense', value: t.averageExpense.toFixed(2), icon: 'fas fa-calculator', colorClass: 'warning' },
    ];
  }

  paymentSourceLabel(source: ShopExpensePaymentMethod): string {
    return '::' + ShopExpensePaymentMethod[source];
  }
}
