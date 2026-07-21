import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopProductCategoriesComponent } from './shop-product-categories.component';
import { ShopProductCategoryEditorComponent } from './shop-product-category-editor.component';
const routes: Routes = [
  { path: '', component: ShopProductCategoriesComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.ProductCategories' } },
  { path: 'create', component: ShopProductCategoryEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.ProductCategories.Create' } },
  { path: 'edit/:id', component: ShopProductCategoryEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.ProductCategories.Edit' } },
];
@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] }) export class ShopProductCategoriesRoutingModule {}
