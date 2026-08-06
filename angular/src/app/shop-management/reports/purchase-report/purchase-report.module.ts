import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { PurchaseReportComponent } from './purchase-report.component';
import { PurchaseReportRoutingModule } from './purchase-report-routing.module';

@NgModule({
  declarations: [PurchaseReportComponent],
  imports: [SharedReportsModule, PurchaseReportRoutingModule],
})
export class PurchaseReportModule {}
