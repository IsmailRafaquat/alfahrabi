import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { GetShopSupplierPayablesReportInput, ShopSupplierPayableReportResultDto } from '../../../proxy/shop-management/reports/models';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-supplier-payables-report',
  standalone: false,
  templateUrl: './supplier-payables-report.component.html',
  styleUrl: './supplier-payables-report.component.scss',
})
export class SupplierPayablesReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.Reports.Export');

  input: GetShopSupplierPayablesReportInput = {
    includeInactiveSuppliers: false,
    sorting: 'outstandingbalance desc',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopSupplierPayableReportResultDto;
  loading = false;
  exporting = false;
  pageIndex = 1;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .getSupplierPayablesReport(this.input)
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
      includeInactiveSuppliers: false,
      sorting: 'outstandingbalance desc',
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
      .exportSupplierPayablesReport(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `supplier-payables-report.${this.extensionFor(format)}`),
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
      { label: '::Suppliers', value: String(t.supplierCount), icon: 'fas fa-truck', colorClass: 'primary' },
      { label: '::WithPayables', value: String(t.suppliersWithPayables), icon: 'fas fa-exclamation-circle', colorClass: 'danger' },
      { label: '::WithAdvance', value: String(t.suppliersWithAdvance), icon: 'fas fa-piggy-bank', colorClass: 'success' },
      { label: '::TotalPayables', value: t.totalPayables.toFixed(2), icon: 'fas fa-money-bill-wave', colorClass: 'danger' },
      { label: '::TotalAdvance', value: t.totalSupplierAdvance.toFixed(2), icon: 'fas fa-coins', colorClass: 'success' },
    ];
  }
}
