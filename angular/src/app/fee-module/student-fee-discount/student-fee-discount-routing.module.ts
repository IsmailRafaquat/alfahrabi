import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StudentFeeDiscountComponent } from './student-fee-discount.component';

const routes: Routes = [{ path: '', component: StudentFeeDiscountComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentFeeDiscountRoutingModule { }
