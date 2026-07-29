import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TaxSummaryReportComponent } from './tax-summary-report.component';

const routes: Routes = [{ path: '', component: TaxSummaryReportComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class TaxSummaryReportRoutingModule {}
