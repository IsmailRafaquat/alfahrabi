import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { GetShopBatchExpiryReportInput, ShopBatchExpiryReportResultDto } from '../../../proxy/shop-management/reports/models';
import { ShopProductBatchStatus, shopProductBatchStatusOptions } from '../../../proxy/shop-management/product-batches/shop-product-batch-status.enum';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-batch-expiry-report',
  standalone: false,
  templateUrl: './batch-expiry-report.component.html',
  styleUrl: './batch-expiry-report.component.scss',
})
export class BatchExpiryReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.Reports.Export');
  readonly batchStatusOptions = shopProductBatchStatusOptions;

  input: GetShopBatchExpiryReportInput = {
    period: ShopReportPeriod.ThisMonth,
    nearExpiryOnly: false,
    expiredOnly: false,
    activeOnly: false,
    hasAvailableStock: false,
    includeBlocked: true,
    sorting: 'ExpiryDate asc',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopBatchExpiryReportResultDto;
  loading = false;
  exporting = false;
  pageIndex = 1;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .getBatchExpiryReport(this.input)
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
      nearExpiryOnly: false,
      expiredOnly: false,
      activeOnly: false,
      hasAvailableStock: false,
      includeBlocked: true,
      sorting: 'ExpiryDate asc',
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
      .exportBatchExpiryReport(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `batch-expiry-report.${this.extensionFor(format)}`),
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
      { label: '::TotalBatches', value: String(t.totalBatches), icon: 'fas fa-boxes', colorClass: 'primary' },
      { label: '::Active', value: String(t.activeBatches), icon: 'fas fa-check-circle', colorClass: 'success' },
      { label: '::NearExpiry', value: String(t.nearExpiryBatches), icon: 'fas fa-hourglass-half', colorClass: 'warning' },
      { label: '::Expired', value: String(t.expiredBatches), icon: 'fas fa-ban', colorClass: 'danger' },
      { label: '::Blocked', value: String(t.blockedBatches), icon: 'fas fa-lock', colorClass: 'danger' },
    ];
  }

  statusLabel(status: ShopProductBatchStatus): string {
    return '::' + ShopProductBatchStatus[status];
  }

  statusVariant(status: ShopProductBatchStatus): 'success' | 'warning' | 'danger' | 'secondary' {
    switch (status) {
      case ShopProductBatchStatus.Active: return 'success';
      case ShopProductBatchStatus.NearExpiry: return 'warning';
      case ShopProductBatchStatus.Expired: return 'danger';
      case ShopProductBatchStatus.Blocked: return 'danger';
      default: return 'secondary';
    }
  }
}
