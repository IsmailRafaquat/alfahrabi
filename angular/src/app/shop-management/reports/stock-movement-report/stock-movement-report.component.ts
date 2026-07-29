import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { ShopStockQuantityDirection } from '../../../proxy/shop-management/reports/shop-stock-quantity-direction.enum';
import { GetShopStockMovementReportInput, ShopStockMovementReportResultDto } from '../../../proxy/shop-management/reports/models';
import { ShopStockTransactionType, shopStockTransactionTypeOptions } from '../../../proxy/shop-management/stock-transactions/shop-stock-transaction-type.enum';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-stock-movement-report',
  standalone: false,
  templateUrl: './stock-movement-report.component.html',
  styleUrl: './stock-movement-report.component.scss',
})
export class StockMovementReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.Reports.Export');
  readonly transactionTypeOptions = shopStockTransactionTypeOptions;
  readonly ShopStockQuantityDirection = ShopStockQuantityDirection;

  input: GetShopStockMovementReportInput = {
    period: ShopReportPeriod.ThisMonth,
    quantityDirection: ShopStockQuantityDirection.All,
    sorting: 'TransactionDate desc',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopStockMovementReportResultDto;
  loading = false;
  exporting = false;
  pageIndex = 1;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .getStockMovementReport(this.input)
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
      quantityDirection: ShopStockQuantityDirection.All,
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
      .exportStockMovementReport(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `stock-movement-report.${this.extensionFor(format)}`),
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
      { label: 'Transactions', value: String(t.transactionCount), icon: 'fas fa-exchange-alt', colorClass: 'primary' },
      { label: 'Quantity In', value: t.totalQuantityIn.toFixed(2), icon: 'fas fa-arrow-down', colorClass: 'success' },
      { label: 'Quantity Out', value: t.totalQuantityOut.toFixed(2), icon: 'fas fa-arrow-up', colorClass: 'danger' },
      { label: 'Net Movement', value: t.netQuantityMovement.toFixed(2), icon: 'fas fa-balance-scale', colorClass: 'info' },
    ];
  }

  typeLabel(type: ShopStockTransactionType): string {
    return '::' + ShopStockTransactionType[type];
  }
}
