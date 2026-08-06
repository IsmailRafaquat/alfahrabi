import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { GetShopCashReportInput, ShopCashReportResultDto } from '../../../proxy/shop-management/reports/models';
import { ShopCashRegisterLookupDto, ShopCashRegisterService } from '../../../proxy/shop-management/cash-registers';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-cash-report',
  standalone: false,
  templateUrl: './cash-report.component.html',
  styleUrl: './cash-report.component.scss',
})
export class CashReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly cashRegisterService = inject(ShopCashRegisterService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.Reports.Export');

  cashRegisters: ShopCashRegisterLookupDto[] = [];

  input: GetShopCashReportInput = {
    period: ShopReportPeriod.ThisMonth,
    sorting: 'TransactionDate desc',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopCashReportResultDto;
  loading = false;
  exporting = false;
  pageIndex = 1;

  ngOnInit(): void {
    this.cashRegisterService.getLookup().subscribe(r => (this.cashRegisters = r.items || []));
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .getCashReport(this.input)
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
      .exportCashReport(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `cash-report.${this.extensionFor(format)}`),
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
    const cards: ReportTotalCard[] = [
      { label: '::OpeningCash', value: t.openingCash.toFixed(2), icon: 'fas fa-cash-register', colorClass: 'primary' },
      { label: '::CashIn', value: t.totalCashIn.toFixed(2), icon: 'fas fa-arrow-down', colorClass: 'success' },
      { label: '::CashOut', value: t.totalCashOut.toFixed(2), icon: 'fas fa-arrow-up', colorClass: 'danger' },
      { label: '::ExpectedClosingCash', value: t.expectedClosingCash.toFixed(2), icon: 'fas fa-balance-scale', colorClass: 'info' },
    ];
    if (t.difference !== undefined && t.difference !== null) {
      cards.push({ label: '::Difference', value: t.difference.toFixed(2), icon: 'fas fa-exclamation-triangle', colorClass: t.difference === 0 ? 'success' : 'warning' });
    }
    return cards;
  }
}
