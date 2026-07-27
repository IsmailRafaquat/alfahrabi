import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopStockCountsComponent } from './shop-stock-counts.component';
import { ShopStockCountEditorComponent } from './shop-stock-count-editor.component';
import { ShopStockCountCountingComponent } from './shop-stock-count-counting.component';
import { ShopStockCountDetailComponent } from './shop-stock-count-detail.component';

const routes: Routes = [
  { path: '', component: ShopStockCountsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.StockCounts' } },
  { path: 'create', component: ShopStockCountEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.StockCounts.Create' } },
  { path: ':id/edit', component: ShopStockCountEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.StockCounts.Edit' } },
  { path: ':id/count', component: ShopStockCountCountingComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.StockCounts.Count' } },
  { path: ':id', component: ShopStockCountDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.StockCounts' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopStockCountsRoutingModule {}
