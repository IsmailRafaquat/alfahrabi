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
    path: 'shop-management/settings',
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'ShopManagement.Settings' },
    loadChildren: () => import('./shop-management/settings/shop-settings.module').then(m => m.ShopSettingsModule),
  },
  {
    path: 'shop-management/product-categories',
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'ShopManagement.ProductCategories' },
    loadChildren: () => import('./shop-management/product-categories/shop-product-categories.module').then(m => m.ShopProductCategoriesModule),
  },
  {
    path: 'shop-management/units', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Units' },
    loadChildren: () => import('./shop-management/units/shop-units.module').then(m => m.ShopUnitsModule),
  },
  {
    path: 'shop-management/products', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Products' },
    loadChildren: () => import('./shop-management/products/shop-products.module').then(m => m.ShopProductsModule),
  },
  {
    path: 'shop-management/suppliers', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Suppliers' },
    loadChildren: () => import('./shop-management/suppliers/shop-suppliers.module').then(m => m.ShopSuppliersModule),
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
    path: 'alfarabi',
    loadChildren: () => import('./alfarabi/alfarabi.module').then(m => m.AlfarabiModule),
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
  { path: 'reports/report/student-fee-report', loadChildren: () => import('./reports/report/student-fee-report/student-fee-report.module').then(m => m.StudentFeeReportModule) },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {})],
  exports: [RouterModule],
})
export class AppRoutingModule {}
