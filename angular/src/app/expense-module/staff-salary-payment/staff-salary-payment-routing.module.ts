import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StaffSalaryPaymentComponent } from './staff-salary-payment.component';

const routes: Routes = [{ path: '', component: StaffSalaryPaymentComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StaffSalaryPaymentRoutingModule { }
