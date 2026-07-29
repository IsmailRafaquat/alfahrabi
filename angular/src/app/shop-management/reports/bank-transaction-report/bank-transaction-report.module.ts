import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { BankTransactionReportComponent } from './bank-transaction-report.component';
import { BankTransactionReportRoutingModule } from './bank-transaction-report-routing.module';

@NgModule({
  declarations: [BankTransactionReportComponent],
  imports: [SharedReportsModule, BankTransactionReportRoutingModule],
})
export class BankTransactionReportModule {}
