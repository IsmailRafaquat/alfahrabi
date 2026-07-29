import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ShopReportExportFormat } from '../../proxy/shop-management/reports/shop-report-export-format.enum';

@Component({
  selector: 'app-report-export-menu',
  standalone: false,
  templateUrl: './report-export-menu.component.html',
  styleUrl: './report-export-menu.component.scss',
})
export class ReportExportMenuComponent {
  @Input() disabled = false;
  @Input() exporting = false;
  @Input() showPrint = true;

  @Output() exportFormat = new EventEmitter<ShopReportExportFormat>();
  @Output() print = new EventEmitter<void>();

  readonly ShopReportExportFormat = ShopReportExportFormat;

  onPrint() {
    this.print.emit();
    window.print();
  }
}
