import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { GetShopProductPerformanceReportInput, ShopProductPerformanceReportResultDto } from '../../../proxy/shop-management/reports/models';
import { ShopProductCategoryLookupDto, ShopProductCategoryService } from '../../../proxy/shop-management/product-categories';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-product-performance-report',
  standalone: false,
  templateUrl: './product-performance-report.component.html',
  styleUrl: './product-performance-report.component.scss',
})
export class ProductPerformanceReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly categoryService = inject(ShopProductCategoryService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.Reports.Export');

  categories: ShopProductCategoryLookupDto[] = [];

  input: GetShopProductPerformanceReportInput = {
    period: ShopReportPeriod.ThisMonth,
    includeInactiveProducts: false,
    soldOnly: false,
    purchasedOnly: false,
    sorting: '',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopProductPerformanceReportResultDto;
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
      .getProductPerformanceReport(this.input)
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
      includeInactiveProducts: false,
      soldOnly: false,
      purchasedOnly: false,
      sorting: '',
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
      .exportProductPerformanceReport(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `product-performance-report.${this.extensionFor(format)}`),
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
      { label: '::Products', value: String(t.productCount), icon: 'fas fa-boxes', colorClass: 'primary' },
      { label: '::PurchasedQuantity', value: t.totalPurchasedQuantity.toFixed(2), icon: 'fas fa-dolly', colorClass: 'info' },
      { label: '::SaleQuantity', value: t.totalSaleQuantity.toFixed(2), icon: 'fas fa-receipt', colorClass: 'success' },
      { label: '::NetSalesAmount', value: t.totalNetSalesAmount.toFixed(2), icon: 'fas fa-coins', colorClass: 'success' },
      { label: '::CurrentStock', value: t.totalCurrentStock.toFixed(2), icon: 'fas fa-cubes', colorClass: 'warning' },
    ];
    if (t.totalCurrentStockValue !== undefined && t.totalCurrentStockValue !== null) {
      cards.push({ label: '::CurrentStockValue', value: t.totalCurrentStockValue.toFixed(2), icon: 'fas fa-money-bill-wave', colorClass: 'info' });
    }
    return cards;
  }
}
