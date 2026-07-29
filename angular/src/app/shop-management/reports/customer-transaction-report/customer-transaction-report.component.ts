import { Component, OnInit, inject } from '@angular/core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { GetShopCustomerTransactionReportInput, ShopCustomerTransactionReportResultDto } from '../../../proxy/shop-management/reports/models';
import { ShopCustomerLedgerReferenceType, shopCustomerLedgerReferenceTypeOptions } from '../../../proxy/shop-management/customer-ledger/shop-customer-ledger-reference-type.enum';
import { ShopCustomerLookupDto, ShopCustomerService } from '../../../proxy/shop-management/customers';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-customer-transaction-report',
  standalone: false,
  templateUrl: './customer-transaction-report.component.html',
  styleUrl: './customer-transaction-report.component.scss',
})
export class CustomerTransactionReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly customerService = inject(ShopCustomerService);
  private readonly toaster = inject(ToasterService);

  readonly transactionTypeOptions = shopCustomerLedgerReferenceTypeOptions;

  customers: ShopCustomerLookupDto[] = [];

  input: GetShopCustomerTransactionReportInput = {
    customerId: '',
    period: ShopReportPeriod.ThisMonth,
    sorting: '',
    skipCount: 0,
    maxResultCount: 20,
  };

  result?: ShopCustomerTransactionReportResultDto;
  loading = false;
  pageIndex = 1;

  ngOnInit(): void {
    this.customerService.getLookup().subscribe(r => (this.customers = r.items || []));
  }

  load(): void {
    if (!this.input.customerId) return;
    this.loading = true;
    this.service
      .getCustomerTransactionReport(this.input)
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
    if (this.input.customerId) this.load();
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

  typeLabel(type: ShopCustomerLedgerReferenceType): string {
    return '::' + ShopCustomerLedgerReferenceType[type];
  }
}
