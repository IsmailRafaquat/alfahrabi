import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { ProfitLossReportComponent } from './profit-loss-report.component';
import { ProfitLossReportRoutingModule } from './profit-loss-report-routing.module';

@NgModule({
  declarations: [ProfitLossReportComponent],
  imports: [SharedReportsModule, ProfitLossReportRoutingModule],
})
export class ProfitLossReportModule {}
