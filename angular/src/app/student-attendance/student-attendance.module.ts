import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StudentAttendanceRoutingModule } from './student-attendance-routing.module';
import { StudentAttendanceComponent } from './student-attendance.component';
import { SharedModule } from '../shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    StudentAttendanceComponent
  ],
  imports: [
    CommonModule,
    StudentAttendanceRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class StudentAttendanceModule { }
