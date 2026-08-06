import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { GetShopBankTransactionReportInput, ShopBankTransactionReportResultDto } from '../../../proxy/shop-management/reports/models';
import { ShopBankAccountLookupDto, ShopBankAccountService } from '../../../proxy/shop-management/bank-accounts';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-bank-transaction-report',
  standalone: false,
  templateUrl: './bank-transaction-report.component.html',
  styleUrl: './bank-transaction-report.component.scss',
})
export class BankTransactionReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly bankAccountService = inject(ShopBankAccountService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.Reports.Export');

  bankAccounts: ShopBankAccountLookupDto[] = [];

  input: GetShopBankTransactionReportInput = {
    period: ShopReportPeriod.ThisMonth,
    sorting: 'TransactionDate desc',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopBankTransactionReportResultDto;
  loading = false;
  exporting = false;
  pageIndex = 1;

  ngOnInit(): void {
    this.bankAccountService.getLookup().subscribe(r => (this.bankAccounts = r.items || []));
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .getBankTransactionReport(this.input)
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
      sorting: 'TransactionDate desc',
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
      .exportBankTransactionReport(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `bank-transaction-report.${this.extensionFor(format)}`),
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
      { label: '::OpeningBalance', value: t.openingBalance.toFixed(2), icon: 'fas fa-university', colorClass: 'primary' },
      { label: '::AmountIn', value: t.totalAmountIn.toFixed(2), icon: 'fas fa-arrow-down', colorClass: 'success' },
      { label: '::AmountOut', value: t.totalAmountOut.toFixed(2), icon: 'fas fa-arrow-up', colorClass: 'danger' },
      { label: '::ClosingBalance', value: t.closingBalance.toFixed(2), icon: 'fas fa-balance-scale', colorClass: 'info' },
    ];
  }
}
