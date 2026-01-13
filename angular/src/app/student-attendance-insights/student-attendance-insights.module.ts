import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StudentAttendanceInsightsRoutingModule } from './student-attendance-insights-routing.module';
import { StudentAttendanceInsightsComponent } from './student-attendance-insights.component';
import { SharedModule } from '../shared/shared.module';
import { PageModule } from '@abp/ng.components/page';
import { ReactiveFormsModule } from '@angular/forms';
import { NzSelectModule } from 'ng-zorro-antd/select';


@NgModule({
  declarations: [
    StudentAttendanceInsightsComponent
  ],
  imports: [
    CommonModule,
    StudentAttendanceInsightsRoutingModule,
    SharedModule,
    PageModule,
    ReactiveFormsModule,
    NzSelectModule
  ]
})
export class StudentAttendanceInsightsModule { }
