import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BatchExpiryReportComponent } from './batch-expiry-report.component';

const routes: Routes = [{ path: '', component: BatchExpiryReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class BatchExpiryReportRoutingModule {}
