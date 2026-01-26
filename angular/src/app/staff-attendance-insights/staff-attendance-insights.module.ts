import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StaffAttendanceInsightsRoutingModule } from './staff-attendance-insights-routing.module';
import { StaffAttendanceInsightsComponent } from './staff-attendance-insights.component';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { NzSelectModule } from 'ng-zorro-antd/select';

@NgModule({
  declarations: [StaffAttendanceInsightsComponent],
  imports: [
    CommonModule,
    StaffAttendanceInsightsRoutingModule,
    SharedModule,
    PageModule,
    ReactiveFormsModule,
    NzSelectModule,
  ],
})
export class StaffAttendanceInsightsModule {}
