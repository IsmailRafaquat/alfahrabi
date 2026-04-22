import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SalaryReportComponent } from './salary-report.component';

const routes: Routes = [{ path: '', component: SalaryReportComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SalaryReportRoutingModule { }
