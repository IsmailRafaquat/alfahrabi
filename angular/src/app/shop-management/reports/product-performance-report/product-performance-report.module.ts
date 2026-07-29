import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../../shared/reports/shared-reports.module';
import { ProductPerformanceReportComponent } from './product-performance-report.component';
import { ProductPerformanceReportRoutingModule } from './product-performance-report-routing.module';

@NgModule({
  declarations: [ProductPerformanceReportComponent],
  imports: [SharedReportsModule, ProductPerformanceReportRoutingModule],
})
export class ProductPerformanceReportModule {}
