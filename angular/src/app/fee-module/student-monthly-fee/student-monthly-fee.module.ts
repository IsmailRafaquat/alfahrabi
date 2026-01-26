import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StudentMonthlyFeeRoutingModule } from './student-monthly-fee-routing.module';
import { StudentMonthlyFeeComponent } from './student-monthly-fee.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    StudentMonthlyFeeComponent
  ],
  imports: [
    CommonModule,
    StudentMonthlyFeeRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class StudentMonthlyFeeModule { }
