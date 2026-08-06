import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { SupplierPayablesReportComponent } from './supplier-payables-report.component';
import { SupplierPayablesReportRoutingModule } from './supplier-payables-report-routing.module';

@NgModule({
  declarations: [SupplierPayablesReportComponent],
  imports: [SharedReportsModule, SupplierPayablesReportRoutingModule],
})
export class SupplierPayablesReportModule {}
