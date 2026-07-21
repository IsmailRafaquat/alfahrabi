import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopProductsComponent } from './shop-products.component';
import { ShopProductsRoutingModule } from './shop-products-routing.module';
import { ShopProductEditorComponent } from './shop-product-editor.component';

@NgModule({
  declarations: [ShopProductsComponent, ShopProductEditorComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopProductsRoutingModule],
})
export class ShopProductsModule {}
