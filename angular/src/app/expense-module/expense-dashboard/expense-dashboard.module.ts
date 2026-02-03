import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ExpenseDashboardRoutingModule } from './expense-dashboard-routing.module';
import { ExpenseDashboardComponent } from './expense-dashboard.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    ExpenseDashboardComponent
  ],
  imports: [
    CommonModule,
    ExpenseDashboardRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class ExpenseDashboardModule { }
