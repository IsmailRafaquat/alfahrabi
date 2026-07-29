import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { BatchExpiryReportComponent } from './batch-expiry-report.component';
import { BatchExpiryReportRoutingModule } from './batch-expiry-report-routing.module';

@NgModule({
  declarations: [BatchExpiryReportComponent],
  imports: [SharedReportsModule, BatchExpiryReportRoutingModule],
})
export class BatchExpiryReportModule {}
