import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CheckFeesDashboardRoutingModule } from './check-fees-dashboard-routing.module';
import { CheckFeesDashboardComponent } from './check-fees-dashboard.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    CheckFeesDashboardComponent
  ],
  imports: [
    CommonModule,
    CheckFeesDashboardRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class CheckFeesDashboardModule { }
