import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StudentMonthlyFeeLineComponent } from './student-monthly-fee-line.component';

const routes: Routes = [{ path: '', component: StudentMonthlyFeeLineComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentMonthlyFeeLineRoutingModule { }
