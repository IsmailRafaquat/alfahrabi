import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopPurchaseOrdersComponent } from './shop-purchase-orders.component';
import { ShopPurchaseOrderEditorComponent } from './shop-purchase-order-editor.component';
import { ShopPurchaseOrderDetailComponent } from './shop-purchase-order-detail.component';

const routes: Routes = [
  { path: '', component: ShopPurchaseOrdersComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.PurchaseOrders' } },
  { path: 'create', component: ShopPurchaseOrderEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.PurchaseOrders.Create' } },
  { path: ':id/edit', component: ShopPurchaseOrderEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.PurchaseOrders.Edit' } },
  { path: ':id', component: ShopPurchaseOrderDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.PurchaseOrders' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopPurchaseOrdersRoutingModule {}
