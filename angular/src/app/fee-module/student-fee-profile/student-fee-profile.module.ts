import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StudentFeeProfileRoutingModule } from './student-fee-profile-routing.module';
import { StudentFeeProfileComponent } from './student-fee-profile.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    StudentFeeProfileComponent
  ],
  imports: [
    CommonModule,
    StudentFeeProfileRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class StudentFeeProfileModule { }
