import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopCustomersComponent } from './shop-customers.component';
import { ShopCustomerEditorComponent } from './shop-customer-editor.component';

const routes: Routes = [
  { path: '', component: ShopCustomersComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Customers' } },
  { path: 'create', component: ShopCustomerEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Customers.Create' } },
  { path: ':id/edit', component: ShopCustomerEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Customers.Edit' } },
  { path: ':id', component: ShopCustomerEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Customers' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopCustomersRoutingModule {}
