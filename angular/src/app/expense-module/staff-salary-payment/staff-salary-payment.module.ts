import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StaffSalaryPaymentRoutingModule } from './staff-salary-payment-routing.module';
import { StaffSalaryPaymentComponent } from './staff-salary-payment.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    StaffSalaryPaymentComponent
  ],
  imports: [
    CommonModule,
    StaffSalaryPaymentRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class StaffSalaryPaymentModule { }
