import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { GetShopPurchaseReportInput, ShopPurchaseReportResultDto } from '../../../proxy/shop-management/reports/models';
import { shopGoodsReceiptPaymentStatusOptions, ShopGoodsReceiptPaymentStatus } from '../../../proxy/shop-management/goods-receipts/shop-goods-receipt-payment-status.enum';
import { ShopSupplierLookupDto, ShopSupplierService } from '../../../proxy/shop-management/suppliers';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-purchase-report',
  standalone: false,
  templateUrl: './purchase-report.component.html',
  styleUrl: './purchase-report.component.scss',
})
export class PurchaseReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly supplierService = inject(ShopSupplierService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.Reports.Export');
  readonly paymentStatusOptions = shopGoodsReceiptPaymentStatusOptions;

  suppliers: ShopSupplierLookupDto[] = [];

  input: GetShopPurchaseReportInput = {
    period: ShopReportPeriod.ThisMonth,
    sorting: 'ReceiptDate desc',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopPurchaseReportResultDto;
  loading = false;
  exporting = false;
  pageIndex = 1;

  ngOnInit(): void {
    this.supplierService.getLookup().subscribe(r => (this.suppliers = r.items || []));
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .getPurchaseReport(this.input)
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
      sorting: 'ReceiptDate desc',
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
      .exportPurchaseReport(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `purchase-report.${this.extensionFor(format)}`),
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
      { label: '::PurchaseCount', value: String(t.purchaseCount), icon: 'fas fa-dolly', colorClass: 'primary' },
      { label: '::GrossPurchases', value: t.grossPurchases.toFixed(2), icon: 'fas fa-coins', colorClass: 'info' },
      { label: '::Discount', value: t.totalDiscount.toFixed(2), icon: 'fas fa-percent', colorClass: 'warning' },
      { label: '::PurchaseReturns', value: t.purchaseReturnAmount.toFixed(2), icon: 'fas fa-undo', colorClass: 'danger' },
      { label: '::FinalNetPurchases', value: t.finalNetPurchases.toFixed(2), icon: 'fas fa-chart-line', colorClass: 'success' },
      { label: '::Paid', value: t.paidAmount.toFixed(2), icon: 'fas fa-hand-holding-usd', colorClass: 'primary' },
      { label: '::Pending', value: t.pendingAmount.toFixed(2), icon: 'fas fa-exclamation-circle', colorClass: 'danger' },
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
