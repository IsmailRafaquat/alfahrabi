import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StudentFeeProfileComponent } from './student-fee-profile.component';

const routes: Routes = [{ path: '', component: StudentFeeProfileComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentFeeProfileRoutingModule { }
