import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StockMovementReportComponent } from './stock-movement-report.component';

const routes: Routes = [{ path: '', component: StockMovementReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class StockMovementReportRoutingModule {}
