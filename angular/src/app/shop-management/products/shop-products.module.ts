import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopProductsComponent } from './shop-products.component';
import { ShopProductsRoutingModule } from './shop-products-routing.module';
import { ShopProductEditorComponent } from './shop-product-editor.component';

@NgModule({
  declarations: [ShopProductsComponent, ShopProductEditorComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopProductsRoutingModule],
})
export class ShopProductsModule {}
