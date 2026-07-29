import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PurchaseReportComponent } from './purchase-report.component';

const routes: Routes = [{ path: '', component: PurchaseReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class PurchaseReportRoutingModule {}
