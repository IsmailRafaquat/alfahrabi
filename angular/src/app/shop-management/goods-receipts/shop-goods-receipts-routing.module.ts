import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopGoodsReceiptsComponent } from './shop-goods-receipts.component';
import { ShopGoodsReceiptEditorComponent } from './shop-goods-receipt-editor.component';
import { ShopGoodsReceiptDetailComponent } from './shop-goods-receipt-detail.component';

const routes: Routes = [
  { path: '', component: ShopGoodsReceiptsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.GoodsReceipts' } },
  { path: 'create', component: ShopGoodsReceiptEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.GoodsReceipts.Create' } },
  { path: 'create/:purchaseOrderId', component: ShopGoodsReceiptEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.GoodsReceipts.Create' } },
  { path: ':id/edit', component: ShopGoodsReceiptEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.GoodsReceipts.Edit' } },
  { path: ':id', component: ShopGoodsReceiptDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.GoodsReceipts' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopGoodsReceiptsRoutingModule {}
