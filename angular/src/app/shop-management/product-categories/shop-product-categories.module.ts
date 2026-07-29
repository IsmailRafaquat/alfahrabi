import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopProductCategoriesComponent } from './shop-product-categories.component';
import { ShopProductCategoriesRoutingModule } from './shop-product-categories-routing.module';
import { ShopProductCategoryEditorComponent } from './shop-product-category-editor.component';
@NgModule({ declarations: [ShopProductCategoriesComponent, ShopProductCategoryEditorComponent], imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopProductCategoriesRoutingModule] }) export class ShopProductCategoriesModule {}
