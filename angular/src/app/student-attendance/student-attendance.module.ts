import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StudentAttendanceRoutingModule } from './student-attendance-routing.module';
import { StudentAttendanceComponent } from './student-attendance.component';
import { SharedModule } from '../shared/shared.module';
import { PageModule } from '@abp/ng.components/page';
import { ClassMarkAttendanceModalComponent } from './class-mark-attendance-modal.component';


@NgModule({
  declarations: [
    StudentAttendanceComponent,
    ClassMarkAttendanceModalComponent
  ],
  imports: [
    CommonModule,
    StudentAttendanceRoutingModule,
    SharedModule,
    PageModule,
  ]
})
export class StudentAttendanceModule { }
