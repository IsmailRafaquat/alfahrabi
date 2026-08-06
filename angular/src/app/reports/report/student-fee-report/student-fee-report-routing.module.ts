import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StudentFeeReportComponent } from './student-fee-report.component';

const routes: Routes = [{ path: '', component: StudentFeeReportComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentFeeReportRoutingModule { }
