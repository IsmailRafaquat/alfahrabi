import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { ShopStockReportStatus, shopStockReportStatusOptions } from '../../../proxy/shop-management/reports/shop-stock-report-status.enum';
import { GetShopStockReportInput, ShopStockReportResultDto } from '../../../proxy/shop-management/reports/models';
import { ShopProductCategoryLookupDto, ShopProductCategoryService } from '../../../proxy/shop-management/product-categories';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-stock-report',
  standalone: false,
  templateUrl: './stock-report.component.html',
  styleUrl: './stock-report.component.scss',
})
export class StockReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly categoryService = inject(ShopProductCategoryService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.Reports.Export');
  readonly stockStatusOptions = shopStockReportStatusOptions;

  categories: ShopProductCategoryLookupDto[] = [];

  input: GetShopStockReportInput = {
    period: ShopReportPeriod.ThisMonth,
    stockStatus: ShopStockReportStatus.All,
    lowStockOnly: false,
    outOfStockOnly: false,
    inStockOnly: false,
    includeInactiveProducts: false,
    sorting: 'Name asc',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopStockReportResultDto;
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
      .getStockReport(this.input)
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
      stockStatus: ShopStockReportStatus.All,
      lowStockOnly: false,
      outOfStockOnly: false,
      inStockOnly: false,
      includeInactiveProducts: false,
      sorting: 'Name asc',
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
      .exportStockReport(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `stock-report.${this.extensionFor(format)}`),
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
      { label: '::TotalProducts', value: String(t.totalProducts), icon: 'fas fa-boxes', colorClass: 'primary' },
      { label: '::InStock', value: String(t.inStockProducts), icon: 'fas fa-check-circle', colorClass: 'success' },
      { label: '::LowStock', value: String(t.lowStockProducts), icon: 'fas fa-exclamation-triangle', colorClass: 'warning' },
      { label: '::OutOfStockProducts', value: String(t.outOfStockProducts), icon: 'fas fa-times-circle', colorClass: 'danger' },
      { label: '::TotalStockQuantity', value: t.totalStockQuantity.toFixed(2), icon: 'fas fa-cubes', colorClass: 'info' },
    ];
    if (t.totalStockValue !== undefined && t.totalStockValue !== null) {
      cards.push({ label: '::TotalStockValue', value: t.totalStockValue.toFixed(2), icon: 'fas fa-coins', colorClass: 'success' });
    }
    return cards;
  }

  stockStatusLabel(status: ShopStockReportStatus): string {
    return '::' + ShopStockReportStatus[status];
  }

  stockStatusVariant(status: ShopStockReportStatus): 'success' | 'warning' | 'danger' | 'secondary' {
    switch (status) {
      case ShopStockReportStatus.InStock: return 'success';
      case ShopStockReportStatus.LowStock: return 'warning';
      case ShopStockReportStatus.OutOfStock: return 'danger';
      case ShopStockReportStatus.NegativeStock: return 'danger';
      default: return 'secondary';
    }
  }
}
