import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-report-print-header',
  standalone: false,
  templateUrl: './report-print-header.component.html',
  styleUrl: './report-print-header.component.scss',
})
export class ReportPrintHeaderComponent {
  @Input() shopName?: string;
  @Input() shopAddress?: string;
  @Input() shopPhone?: string;
  @Input() reportTitleKey = '';
  @Input() dateRangeText = '';
  @Input() appliedFiltersText = '';

  readonly generatedDate = new Date();
}
