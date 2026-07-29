import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StockReportComponent } from './stock-report.component';

const routes: Routes = [{ path: '', component: StockReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class StockReportRoutingModule {}
