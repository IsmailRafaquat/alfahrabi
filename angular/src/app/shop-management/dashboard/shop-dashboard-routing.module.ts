import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopDashboardComponent } from './shop-dashboard.component';

const routes: Routes = [
  { path: '', component: ShopDashboardComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Dashboard' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopDashboardRoutingModule {}
