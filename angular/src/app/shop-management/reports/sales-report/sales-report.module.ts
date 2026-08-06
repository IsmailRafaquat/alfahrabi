import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { SalesReportComponent } from './sales-report.component';
import { SalesReportRoutingModule } from './sales-report-routing.module';

@NgModule({
  declarations: [SalesReportComponent],
  imports: [SharedReportsModule, SalesReportRoutingModule],
})
export class SalesReportModule {}
