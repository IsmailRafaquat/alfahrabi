import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopSaleReturnsComponent } from './shop-sale-returns.component';
import { ShopSaleReturnEditorComponent } from './shop-sale-return-editor.component';
import { ShopSaleReturnDetailComponent } from './shop-sale-return-detail.component';

const routes: Routes = [
  { path: '', component: ShopSaleReturnsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.SaleReturns' } },
  { path: 'create/:saleId', component: ShopSaleReturnEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.SaleReturns.Create' } },
  { path: ':id/edit', component: ShopSaleReturnEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.SaleReturns.Edit' } },
  { path: ':id', component: ShopSaleReturnDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.SaleReturns' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopSaleReturnsRoutingModule {}
