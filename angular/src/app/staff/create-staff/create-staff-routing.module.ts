import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateStaffComponent } from './create-staff.component';

const routes: Routes = [{ path: '', component: CreateStaffComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CreateStaffRoutingModule { }
