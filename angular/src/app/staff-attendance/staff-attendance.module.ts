import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StaffAttendanceRoutingModule } from './staff-attendance-routing.module';
import { StaffAttendanceComponent } from './staff-attendance.component';
import { SharedModule } from '../shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    StaffAttendanceComponent
  ],
  imports: [
    CommonModule,
    StaffAttendanceRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class StaffAttendanceModule { }
