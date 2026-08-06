import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopStockAdjustmentsComponent } from './shop-stock-adjustments.component';
import { ShopStockAdjustmentEditorComponent } from './shop-stock-adjustment-editor.component';
import { ShopStockAdjustmentDetailComponent } from './shop-stock-adjustment-detail.component';

const routes: Routes = [
  { path: '', component: ShopStockAdjustmentsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.StockAdjustments' } },
  { path: 'create', component: ShopStockAdjustmentEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.StockAdjustments.Create' } },
  { path: ':id/edit', component: ShopStockAdjustmentEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.StockAdjustments.Edit' } },
  { path: ':id', component: ShopStockAdjustmentDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.StockAdjustments' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopStockAdjustmentsRoutingModule {}
