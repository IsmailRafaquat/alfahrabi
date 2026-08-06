import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { CustomerReceivablesReportComponent } from './customer-receivables-report.component';
import { CustomerReceivablesReportRoutingModule } from './customer-receivables-report-routing.module';

@NgModule({
  declarations: [CustomerReceivablesReportComponent],
  imports: [SharedReportsModule, CustomerReceivablesReportRoutingModule],
})
export class CustomerReceivablesReportModule {}
