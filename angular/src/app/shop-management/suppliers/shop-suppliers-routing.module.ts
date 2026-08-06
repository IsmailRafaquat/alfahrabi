import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopSuppliersComponent } from './shop-suppliers.component';
import { ShopSupplierEditorComponent } from './shop-supplier-editor.component';

const routes: Routes = [
  { path: '', component: ShopSuppliersComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Suppliers' } },
  { path: 'create', component: ShopSupplierEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Suppliers.Create' } },
  { path: 'edit/:id', component: ShopSupplierEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Suppliers.Edit' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopSuppliersRoutingModule {}
