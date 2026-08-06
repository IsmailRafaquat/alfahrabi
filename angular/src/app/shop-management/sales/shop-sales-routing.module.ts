import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopSalesComponent } from './shop-sales.component';
import { ShopSaleEditorComponent } from './shop-sale-editor.component';
import { ShopSaleDetailComponent } from './shop-sale-detail.component';

const routes: Routes = [
  { path: '', component: ShopSalesComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Sales' } },
  { path: 'create', component: ShopSaleEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Sales.Create' } },
  { path: ':id/edit', component: ShopSaleEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Sales.Edit' } },
  { path: ':id', component: ShopSaleDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Sales' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopSalesRoutingModule {}
