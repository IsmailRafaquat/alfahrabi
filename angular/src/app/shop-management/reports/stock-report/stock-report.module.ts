import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { StockReportComponent } from './stock-report.component';
import { StockReportRoutingModule } from './stock-report-routing.module';

@NgModule({
  declarations: [StockReportComponent],
  imports: [SharedReportsModule, StockReportRoutingModule],
})
export class StockReportModule {}
