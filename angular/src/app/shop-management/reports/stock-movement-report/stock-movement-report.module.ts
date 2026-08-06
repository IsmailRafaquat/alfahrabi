import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { StockMovementReportComponent } from './stock-movement-report.component';
import { StockMovementReportRoutingModule } from './stock-movement-report-routing.module';

@NgModule({
  declarations: [StockMovementReportComponent],
  imports: [SharedReportsModule, StockMovementReportRoutingModule],
})
export class StockMovementReportModule {}
