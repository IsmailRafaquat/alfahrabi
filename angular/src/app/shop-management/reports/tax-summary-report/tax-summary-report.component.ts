import { Component, OnInit, inject } from '@angular/core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { ShopReportService } from '../../../proxy/shop-management/reports/shop-report.service';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { GetShopTaxSummaryReportInput, ShopTaxSummaryReportDto } from '../../../proxy/shop-management/reports/models';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

@Component({
  selector: 'app-tax-summary-report',
  standalone: false,
  templateUrl: './tax-summary-report.component.html',
  styleUrl: './tax-summary-report.component.scss',
})
export class TaxSummaryReportComponent implements OnInit {
  private readonly service = inject(ShopReportService);
  private readonly toaster = inject(ToasterService);

  input: GetShopTaxSummaryReportInput = {
    period: ShopReportPeriod.ThisMonth,
  };

  result?: ShopTaxSummaryReportDto;
  loading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .getTaxSummaryReport(this.input)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: r => (this.result = r),
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  onFilterChange(): void {
    this.load();
  }

  reset(): void {
    this.input = { period: ShopReportPeriod.ThisMonth };
    this.load();
  }

  get totalsCards(): ReportTotalCard[] {
    const t = this.result;
    if (!t) return [];
    return [
      { label: '::SalesTaxCollected', value: t.salesTaxCollected.toFixed(2), icon: 'fas fa-receipt', colorClass: 'success' },
      { label: '::SalesReturnTaxReversed', value: t.salesReturnTaxReversed.toFixed(2), icon: 'fas fa-undo', colorClass: 'warning' },
      { label: '::NetSalesTax', value: t.netSalesTax.toFixed(2), icon: 'fas fa-percentage', colorClass: 'primary' },
      { label: '::PurchaseTaxPaid', value: t.purchaseTaxPaid.toFixed(2), icon: 'fas fa-file-invoice-dollar', colorClass: 'info' },
      { label: '::PurchaseReturnTaxReversed', value: t.purchaseReturnTaxReversed.toFixed(2), icon: 'fas fa-undo', colorClass: 'warning' },
      { label: '::NetPurchaseTax', value: t.netPurchaseTax.toFixed(2), icon: 'fas fa-percentage', colorClass: 'info' },
      { label: '::NetTaxPosition', value: t.netTaxPosition.toFixed(2), icon: 'fas fa-balance-scale', colorClass: t.netTaxPosition >= 0 ? 'success' : 'danger' },
    ];
  }
}
