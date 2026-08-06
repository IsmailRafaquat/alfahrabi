import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProfitLossReportComponent } from './profit-loss-report.component';

const routes: Routes = [{ path: '', component: ProfitLossReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ProfitLossReportRoutingModule {}
