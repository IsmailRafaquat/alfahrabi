import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopProductsComponent } from './shop-products.component';
import { ShopProductEditorComponent } from './shop-product-editor.component';

const routes: Routes = [
  { path: '', component: ShopProductsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Products' } },
  { path: 'create', component: ShopProductEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Products.Create' } },
  { path: 'edit/:id', component: ShopProductEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Products.Edit' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopProductsRoutingModule {}
