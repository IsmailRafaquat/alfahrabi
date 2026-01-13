import { authGuard, permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadChildren: () => import('./home/home.module').then(m => m.HomeModule),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(m => m.AccountModule.forLazy()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(m => m.IdentityModule.forLazy()),
  },
  {
    path: 'tenant-management',
    loadChildren: () =>
      import('@abp/ng.tenant-management').then(m => m.TenantManagementModule.forLazy()),
  },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(m => m.SettingManagementModule.forLazy()),
  },
  { path: 'dashboards', loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule) },
  { path: 'students', loadChildren: () => import('./student/student.module').then(m => m.StudentModule) },
  { path: 'staffs', loadChildren: () => import('./staff/staff.module').then(m => m.StaffModule) },
  { path: 'subjects', loadChildren: () => import('./subject/subject.module').then(m => m.SubjectModule) },
  { path: 'create-student', loadChildren: () => import('./student/create-student/create-student.module').then(m => m.CreateStudentModule) },
  { path: 'create-staff', loadChildren: () => import('./staff/create-staff/create-staff.module').then(m => m.CreateStaffModule) },
  { path: 'student-attendance', loadChildren: () => import('./student-attendance/student-attendance.module').then(m => m.StudentAttendanceModule) },
  { path: 'student-attendance-insights', loadChildren: () => import('./student-attendance-insights/student-attendance-insights.module').then(m => m.StudentAttendanceInsightsModule) },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {})],
  exports: [RouterModule],
})
export class AppRoutingModule {}
