import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DashboardRoutingModule } from './dashboard-routing.module';
import { DashboardComponent } from './dashboard.component';
import { SharedModule } from '../shared/shared.module';
import { PageModule } from '@abp/ng.components/page';
import { TopbarLayoutModule } from '../components/topbar-layout/topbar-layout.module';


@NgModule({
  declarations: [
    DashboardComponent
  ],
  imports: [
    CommonModule,
    DashboardRoutingModule,
    SharedModule,
    PageModule,
    TopbarLayoutModule
  ]
})
export class DashboardModule { }
