import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StaffAttendanceInsightsComponent } from './staff-attendance-insights.component';

const routes: Routes = [{ path: '', component: StaffAttendanceInsightsComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StaffAttendanceInsightsRoutingModule { }
