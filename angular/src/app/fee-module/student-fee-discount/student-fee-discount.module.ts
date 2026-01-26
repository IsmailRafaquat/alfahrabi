import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StudentFeeDiscountRoutingModule } from './student-fee-discount-routing.module';
import { StudentFeeDiscountComponent } from './student-fee-discount.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    StudentFeeDiscountComponent
  ],
  imports: [
    CommonModule,
    StudentFeeDiscountRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class StudentFeeDiscountModule { }
