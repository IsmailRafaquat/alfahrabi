import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { CashReportComponent } from './cash-report.component';
import { CashReportRoutingModule } from './cash-report-routing.module';

@NgModule({
  declarations: [CashReportComponent],
  imports: [SharedReportsModule, CashReportRoutingModule],
})
export class CashReportModule {}
