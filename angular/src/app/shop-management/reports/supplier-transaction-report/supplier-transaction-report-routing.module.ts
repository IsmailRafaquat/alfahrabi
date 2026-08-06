import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SupplierTransactionReportComponent } from './supplier-transaction-report.component';

const routes: Routes = [{ path: '', component: SupplierTransactionReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class SupplierTransactionReportRoutingModule {}
