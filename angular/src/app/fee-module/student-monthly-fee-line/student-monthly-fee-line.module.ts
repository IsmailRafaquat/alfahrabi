import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StudentMonthlyFeeLineRoutingModule } from './student-monthly-fee-line-routing.module';
import { StudentMonthlyFeeLineComponent } from './student-monthly-fee-line.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';
import { NzCollapseModule } from 'ng-zorro-antd/collapse';


@NgModule({
  declarations: [
    StudentMonthlyFeeLineComponent
  ],
  imports: [
    CommonModule,
    StudentMonthlyFeeLineRoutingModule,
    SharedModule,
    PageModule,
    NzCollapseModule
  ]
})
export class StudentMonthlyFeeLineModule { }
