import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopSuppliersComponent } from './shop-suppliers.component';
import { ShopSuppliersRoutingModule } from './shop-suppliers-routing.module';
import { ShopSupplierEditorComponent } from './shop-supplier-editor.component';

@NgModule({
  declarations: [ShopSuppliersComponent, ShopSupplierEditorComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopSuppliersRoutingModule],
})
export class ShopSuppliersModule {}
