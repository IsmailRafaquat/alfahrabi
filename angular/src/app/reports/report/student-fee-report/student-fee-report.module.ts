import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StudentFeeReportRoutingModule } from './student-fee-report-routing.module';
import { StudentFeeReportComponent } from './student-fee-report.component';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    component: StudentFeeReportComponent,
  },
];

@NgModule({
   imports: [RouterModule.forChild(routes)],
   exports: [RouterModule],
})
export class StudentFeeReportModule { }
