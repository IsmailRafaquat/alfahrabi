import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { ShopSalesReportGroupBy, shopSalesReportGroupByOptions } from '../../../proxy/shop-management/reports/shop-sales-report-group-by.enum';
import { GetShopSalesReportInput, ShopSalesReportResultDto } from '../../../proxy/shop-management/reports/models';
import { shopGoodsReceiptPaymentStatusOptions, ShopGoodsReceiptPaymentStatus } from '../../../proxy/shop-management/goods-receipts/shop-goods-receipt-payment-status.enum';
import { ShopCustomerLookupDto, ShopCustomerService } from '../../../proxy/shop-management/customers';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-sales-report',
  standalone: false,
  templateUrl: './sales-report.component.html',
  styleUrl: './sales-report.component.scss',
})
export class SalesReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly customerService = inject(ShopCustomerService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.Reports.Export');
  readonly paymentStatusOptions = shopGoodsReceiptPaymentStatusOptions;
  readonly groupByOptions = shopSalesReportGroupByOptions;
  readonly ShopSalesReportGroupBy = ShopSalesReportGroupBy;

  customers: ShopCustomerLookupDto[] = [];

  input: GetShopSalesReportInput = {
    period: ShopReportPeriod.ThisMonth,
    groupBy: ShopSalesReportGroupBy.None,
    sorting: 'SaleDate desc',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopSalesReportResultDto;
  loading = false;
  exporting = false;
  pageIndex = 1;

  ngOnInit(): void {
    this.customerService.getLookup().subscribe(r => (this.customers = r.items || []));
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .getSalesReport(this.input)
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
      groupBy: ShopSalesReportGroupBy.None,
      sorting: 'SaleDate desc',
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
      .exportSalesReport(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `sales-report.${this.extensionFor(format)}`),
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
      { label: 'Sale Count', value: String(t.saleCount), icon: 'fas fa-receipt', colorClass: 'primary' },
      { label: 'Gross Sales', value: t.grossSales.toFixed(2), icon: 'fas fa-coins', colorClass: 'success' },
      { label: 'Discount', value: t.totalDiscount.toFixed(2), icon: 'fas fa-percent', colorClass: 'warning' },
      { label: 'Tax', value: t.totalTax.toFixed(2), icon: 'fas fa-file-invoice', colorClass: 'info' },
      { label: 'Sale Returns', value: t.saleReturnAmount.toFixed(2), icon: 'fas fa-undo', colorClass: 'danger' },
      { label: 'Final Net Sales', value: t.finalNetSales.toFixed(2), icon: 'fas fa-chart-line', colorClass: 'success' },
      { label: 'Paid', value: t.paidAmount.toFixed(2), icon: 'fas fa-hand-holding-usd', colorClass: 'primary' },
      { label: 'Pending', value: t.pendingAmount.toFixed(2), icon: 'fas fa-exclamation-circle', colorClass: 'danger' },
      { label: 'Avg Sale Value', value: t.averageSaleValue.toFixed(2), icon: 'fas fa-calculator', colorClass: 'info' },
    ];
  }

  paymentStatusLabel(status: ShopGoodsReceiptPaymentStatus): string {
    return '::' + ShopGoodsReceiptPaymentStatus[status];
  }

  paymentStatusVariant(status: ShopGoodsReceiptPaymentStatus): 'success' | 'warning' | 'danger' {
    switch (status) {
      case ShopGoodsReceiptPaymentStatus.Paid: return 'success';
      case ShopGoodsReceiptPaymentStatus.PartiallyPaid: return 'warning';
      default: return 'danger';
    }
  }
}
