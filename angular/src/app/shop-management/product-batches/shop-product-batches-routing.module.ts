import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopProductBatchesComponent } from './shop-product-batches.component';
import { ShopProductBatchDetailComponent } from './shop-product-batch-detail.component';

const routes: Routes = [
  { path: '', component: ShopProductBatchesComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.ProductBatches.View' } },
  { path: ':id', component: ShopProductBatchDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.ProductBatches.View' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopProductBatchesRoutingModule {}
