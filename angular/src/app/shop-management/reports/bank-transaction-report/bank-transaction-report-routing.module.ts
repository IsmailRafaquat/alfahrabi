import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BankTransactionReportComponent } from './bank-transaction-report.component';

const routes: Routes = [{ path: '', component: BankTransactionReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class BankTransactionReportRoutingModule {}
