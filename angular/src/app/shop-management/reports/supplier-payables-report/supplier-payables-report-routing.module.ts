import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SupplierPayablesReportComponent } from './supplier-payables-report.component';

const routes: Routes = [{ path: '', component: SupplierPayablesReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class SupplierPayablesReportRoutingModule {}
