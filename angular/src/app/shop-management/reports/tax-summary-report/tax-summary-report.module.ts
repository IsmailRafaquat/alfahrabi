import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { TaxSummaryReportComponent } from './tax-summary-report.component';
import { TaxSummaryReportRoutingModule } from './tax-summary-report-routing.module';

@NgModule({
  declarations: [TaxSummaryReportComponent],
  imports: [SharedReportsModule, TaxSummaryReportRoutingModule],
})
export class TaxSummaryReportModule {}
