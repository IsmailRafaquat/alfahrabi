import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ExpenseDashboardComponent } from './expense-dashboard.component';

const routes: Routes = [{ path: '', component: ExpenseDashboardComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ExpenseDashboardRoutingModule { }
