import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ShopReportPeriod, shopReportPeriodOptions } from '../../proxy/shop-management/reports/shop-report-period.enum';

@Component({
  selector: 'app-report-period-filter',
  standalone: false,
  templateUrl: './report-period-filter.component.html',
  styleUrl: './report-period-filter.component.scss',
})
export class ReportPeriodFilterComponent {
  @Input() period: ShopReportPeriod = ShopReportPeriod.ThisMonth;
  @Input() dateFrom?: string;
  @Input() dateTo?: string;
  @Input() disabled = false;

  @Output() periodChange = new EventEmitter<ShopReportPeriod>();
  @Output() dateFromChange = new EventEmitter<string>();
  @Output() dateToChange = new EventEmitter<string>();
  @Output() apply = new EventEmitter<void>();

  readonly ShopReportPeriod = ShopReportPeriod;
  readonly periodOptions = shopReportPeriodOptions;

  onPeriodChange(value: ShopReportPeriod) {
    this.period = value;
    this.periodChange.emit(value);
    if (value !== ShopReportPeriod.Custom) {
      this.apply.emit();
    }
  }

  onDateFromChange(value: string) {
    this.dateFrom = value;
    this.dateFromChange.emit(value);
  }

  onDateToChange(value: string) {
    this.dateTo = value;
    this.dateToChange.emit(value);
  }
}
