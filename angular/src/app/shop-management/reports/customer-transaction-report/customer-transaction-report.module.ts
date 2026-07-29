import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { CustomerTransactionReportComponent } from './customer-transaction-report.component';
import { CustomerTransactionReportRoutingModule } from './customer-transaction-report-routing.module';

@NgModule({
  declarations: [CustomerTransactionReportComponent],
  imports: [SharedReportsModule, CustomerTransactionReportRoutingModule],
})
export class CustomerTransactionReportModule {}
