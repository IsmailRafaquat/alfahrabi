import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { SupplierTransactionReportComponent } from './supplier-transaction-report.component';
import { SupplierTransactionReportRoutingModule } from './supplier-transaction-report-routing.module';

@NgModule({
  declarations: [SupplierTransactionReportComponent],
  imports: [SharedReportsModule, SupplierTransactionReportRoutingModule],
})
export class SupplierTransactionReportModule {}
