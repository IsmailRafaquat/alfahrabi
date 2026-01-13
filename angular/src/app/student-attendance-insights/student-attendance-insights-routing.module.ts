import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StudentAttendanceInsightsComponent } from './student-attendance-insights.component';

const routes: Routes = [{ path: '', component: StudentAttendanceInsightsComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentAttendanceInsightsRoutingModule { }
