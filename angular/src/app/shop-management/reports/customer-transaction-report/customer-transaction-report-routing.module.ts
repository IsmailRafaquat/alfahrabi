import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CustomerTransactionReportComponent } from './customer-transaction-report.component';

const routes: Routes = [{ path: '', component: CustomerTransactionReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class CustomerTransactionReportRoutingModule {}
