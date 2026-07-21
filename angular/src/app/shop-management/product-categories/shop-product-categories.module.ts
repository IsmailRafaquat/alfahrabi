import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopProductCategoriesComponent } from './shop-product-categories.component';
import { ShopProductCategoriesRoutingModule } from './shop-product-categories-routing.module';
import { ShopProductCategoryEditorComponent } from './shop-product-category-editor.component';
@NgModule({ declarations: [ShopProductCategoriesComponent, ShopProductCategoryEditorComponent], imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopProductCategoriesRoutingModule] }) export class ShopProductCategoriesModule {}
