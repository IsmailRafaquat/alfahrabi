import { Component, OnInit, inject } from '@angular/core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { GetShopSupplierTransactionReportInput, ShopSupplierTransactionReportResultDto } from '../../../proxy/shop-management/reports/models';
import { ShopSupplierLedgerReferenceType, shopSupplierLedgerReferenceTypeOptions } from '../../../proxy/shop-management/supplier-ledger/shop-supplier-ledger-reference-type.enum';
import { ShopSupplierLookupDto, ShopSupplierService } from '../../../proxy/shop-management/suppliers';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-supplier-transaction-report',
  standalone: false,
  templateUrl: './supplier-transaction-report.component.html',
  styleUrl: './supplier-transaction-report.component.scss',
})
export class SupplierTransactionReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly supplierService = inject(ShopSupplierService);
  private readonly toaster = inject(ToasterService);

  readonly transactionTypeOptions = shopSupplierLedgerReferenceTypeOptions;

  suppliers: ShopSupplierLookupDto[] = [];

  input: GetShopSupplierTransactionReportInput = {
    supplierId: '',
    period: ShopReportPeriod.ThisMonth,
    sorting: '',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopSupplierTransactionReportResultDto;
  loading = false;
  pageIndex = 1;

  ngOnInit(): void {
    this.supplierService.getLookup().subscribe(r => (this.suppliers = r.items || []));
  }

  load(): void {
    if (!this.input.supplierId) return;
    this.loading = true;
    this.service
      .getSupplierTransactionReport(this.input)
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
    this.input.period = ShopReportPeriod.ThisMonth;
    this.input.dateFrom = undefined;
    this.input.dateTo = undefined;
    this.input.transactionType = undefined;
    this.input.referenceNumber = undefined;
    this.input.filter = undefined;
    this.input.skipCount = 0;
    this.pageIndex = 1;
    if (this.input.supplierId) this.load();
  }

  onPageChange(page: number): void {
    this.pageIndex = page;
    this.input.skipCount = (page - 1) * (this.input.maxResultCount || 20);
    this.load();
  }

  get totalsCards(): ReportTotalCard[] {
    const t = this.result?.totals;
    if (!t) return [];
    return [
      { label: 'Opening Balance', value: t.openingBalance.toFixed(2), icon: 'fas fa-book', colorClass: 'primary' },
      { label: 'Total Debit', value: t.totalDebit.toFixed(2), icon: 'fas fa-arrow-up', colorClass: 'danger' },
      { label: 'Total Credit', value: t.totalCredit.toFixed(2), icon: 'fas fa-arrow-down', colorClass: 'success' },
      { label: 'Closing Balance', value: t.closingBalance.toFixed(2), icon: 'fas fa-balance-scale', colorClass: 'info' },
    ];
  }

  typeLabel(type: ShopSupplierLedgerReferenceType): string {
    return '::' + ShopSupplierLedgerReferenceType[type];
  }
}
