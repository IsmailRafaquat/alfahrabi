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
  {
    path: 'dashboards',
    loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule),
  },
  {
    path: 'students',
    loadChildren: () => import('./student/student.module').then(m => m.StudentModule),
  },
  { path: 'staffs', loadChildren: () => import('./staff/staff.module').then(m => m.StaffModule) },
  {
    path: 'subjects',
    loadChildren: () => import('./subject/subject.module').then(m => m.SubjectModule),
  },
  {
    path: 'create-student',
    loadChildren: () =>
      import('./student/create-student/create-student.module').then(m => m.CreateStudentModule),
  },
  {
    path: 'create-staff',
    loadChildren: () =>
      import('./staff/create-staff/create-staff.module').then(m => m.CreateStaffModule),
  },
  {
    path: 'student-attendance',
    loadChildren: () =>
      import('./student-attendance/student-attendance.module').then(m => m.StudentAttendanceModule),
  },
  {
    path: 'student-attendance-insights',
    loadChildren: () =>
      import('./student-attendance-insights/student-attendance-insights.module').then(
        m => m.StudentAttendanceInsightsModule,
      ),
  },
  {
    path: 'staff-attendances',
    loadChildren: () =>
      import('./staff-attendance/staff-attendance.module').then(m => m.StaffAttendanceModule),
  },
  {
    path: 'staff-attendance-insights',
    loadChildren: () =>
      import('./staff-attendance-insights/staff-attendance-insights.module').then(
        m => m.StaffAttendanceInsightsModule,
      ),
  },
  {
    path: 'fee-heads',
    loadChildren: () => import('./fee-module/fee-head/fee-head.module').then(m => m.FeeHeadModule),
  },
  {
    path: 'fee-heads',
    loadChildren: () => import('./fee-module/fee-head/fee-head.module').then(m => m.FeeHeadModule),
  },
  {
    path: 'fee-structures',
    loadChildren: () =>
      import('./fee-module/fee-structure/fee-structure.module').then(m => m.FeeStructureModule),
  },
  {
    path: 'fee-structure-items',
    loadChildren: () =>
      import('./fee-module/fee-structure-item/fee-structure-item.module').then(
        m => m.FeeStructureItemModule,
      ),
  },
  {
    path: 'student-fee-profiles',
    loadChildren: () =>
      import('./fee-module/student-fee-profile/student-fee-profile.module').then(
        m => m.StudentFeeProfileModule,
      ),
  },
  {
    path: 'student-fee-discounts',
    loadChildren: () =>
      import('./fee-module/student-fee-discount/student-fee-discount.module').then(
        m => m.StudentFeeDiscountModule,
      ),
  },
  {
    path: 'late-fee-polices',
    loadChildren: () =>
      import('./fee-module/late-fee-policy/late-fee-policy.module').then(
        m => m.LateFeePolicyModule,
      ),
  },
  {
    path: 'student-monthly-fees',
    loadChildren: () =>
      import('./fee-module/student-monthly-fee/student-monthly-fee.module').then(
        m => m.StudentMonthlyFeeModule,
      ),
  },
  {
    path: 'student-monthly-fee-lines',
    loadChildren: () =>
      import('./fee-module/student-monthly-fee-line/student-monthly-fee-line.module').then(
        m => m.StudentMonthlyFeeLineModule,
      ),
  },
  {
    path: 'check-fees-dashboard',
    loadChildren: () =>
      import('./fee-module/check-fees-dashboard/check-fees-dashboard.module').then(
        m => m.CheckFeesDashboardModule,
      ),
  },
  {
    path: 'expense-categories',
    loadChildren: () =>
      import('./expense-module/expense-category/expense-category.module').then(
        m => m.ExpenseCategoryModule,
      ),
  },
  {
    path: 'expense-entries',
    loadChildren: () =>
      import('./expense-module/expense-entry/expense-entry.module').then(m => m.ExpenseEntryModule),
  },
  {
    path: 'staff-salary-payments',
    loadChildren: () =>
      import('./expense-module/staff-salary-payment/staff-salary-payment.module').then(
        m => m.StaffSalaryPaymentModule,
      ),
  },
  {
    path: 'expense-dashboard',
    loadChildren: () =>
      import('./expense-module/expense-dashboard/expense-dashboard.module').then(
        m => m.ExpenseDashboardModule,
      ),
  },
  {
    path: 'reports/report',
    loadChildren: () => import('./reports/report/report.module').then(m => m.ReportModule),
  },
  {
    path: 'reports/report/expense-report',
    loadComponent: () =>
      import('./reports/report/expense-report/expense-report.component').then(
        m => m.ExpenseReportComponent,
      ),
  },
  {
    path: 'components/topbar-layout',
    loadChildren: () =>
      import('./components/topbar-layout/topbar-layout.module').then(m => m.TopbarLayoutModule),
  },
  { path: 'reports/report/salary-report', loadChildren: () => import('./reports/report/salary-report/salary-report.module').then(m => m.SalaryReportModule) },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {})],
  exports: [RouterModule],
})
export class AppRoutingModule {}
