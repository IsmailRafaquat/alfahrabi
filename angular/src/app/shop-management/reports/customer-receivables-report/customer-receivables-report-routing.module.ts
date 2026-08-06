import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CustomerReceivablesReportComponent } from './customer-receivables-report.component';

const routes: Routes = [{ path: '', component: CustomerReceivablesReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class CustomerReceivablesReportRoutingModule {}
