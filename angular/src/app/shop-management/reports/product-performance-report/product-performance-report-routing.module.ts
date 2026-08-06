import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductPerformanceReportComponent } from './product-performance-report.component';

const routes: Routes = [{ path: '', component: ProductPerformanceReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ProductPerformanceReportRoutingModule {}
