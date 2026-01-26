import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CheckFeesDashboardComponent } from './check-fees-dashboard.component';

const routes: Routes = [{ path: '', component: CheckFeesDashboardComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CheckFeesDashboardRoutingModule { }
