import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopSettingsComponent } from './shop-settings.component';
import { ShopPrintSettingsComponent } from './shop-print-settings.component';
const routes: Routes = [
  { path: '', component: ShopSettingsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Settings' } },
  { path: 'print', component: ShopPrintSettingsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Print' } },
];
@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopSettingsRoutingModule {}
