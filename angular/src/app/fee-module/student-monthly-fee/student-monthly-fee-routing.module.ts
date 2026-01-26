import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StudentMonthlyFeeComponent } from './student-monthly-fee.component';

const routes: Routes = [{ path: '', component: StudentMonthlyFeeComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentMonthlyFeeRoutingModule { }
