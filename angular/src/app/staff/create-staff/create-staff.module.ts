import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CreateStaffRoutingModule } from './create-staff-routing.module';
import { CreateStaffComponent } from './create-staff.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    CreateStaffComponent
  ],
  imports: [
    CommonModule,
    CreateStaffRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class CreateStaffModule { }
