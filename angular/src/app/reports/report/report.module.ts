import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ReportRoutingModule } from './report-routing.module';
import { ReportComponent } from './report.component';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from 'src/app/shared/shared.module';


@NgModule({
  declarations: [
    ReportComponent
  ],
  imports: [
    CommonModule,
    ReportRoutingModule,
    PageModule,
    SharedModule
  ]
})
export class ReportModule { }
