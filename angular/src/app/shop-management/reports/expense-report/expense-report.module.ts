import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { ExpenseReportComponent } from './expense-report.component';
import { ExpenseReportRoutingModule } from './expense-report-routing.module';

@NgModule({
  declarations: [ExpenseReportComponent],
  imports: [SharedReportsModule, ExpenseReportRoutingModule],
})
export class ExpenseReportModule {}
