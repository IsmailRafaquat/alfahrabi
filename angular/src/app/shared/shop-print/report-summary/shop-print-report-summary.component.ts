import { Component, Input } from '@angular/core';

export interface ShopPrintReportKpi {
  label: string;
  value: string;
}

export interface ShopPrintReportTopItem {
  label: string;
  value: string;
}

// Generic compact thermal summary for Reports/Profit&Loss - each report page adapts its own
// already-loaded aggregate data into this shape locally (no backend call, no per-report bespoke
// thermal HTML). A4 printing for reports is untouched - it keeps using the existing
// SharedReportsModule whole-page print, this component is thermal-only.
@Component({
  selector: 'shop-print-report-summary',
  standalone: false,
  templateUrl: './shop-print-report-summary.component.html',
  styleUrls: ['./shop-print-report-summary.component.scss'],
})
export class ShopPrintReportSummaryComponent {
  @Input() title = '';
  @Input() dateRangeText = '';
  @Input() shopName = '';
  @Input() kpis: ShopPrintReportKpi[] = [];
  @Input() topItemsTitle?: string;
  @Input() topItems?: ShopPrintReportTopItem[];
  @Input() generatedAt = new Date();
}
