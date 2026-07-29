import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopSuppliersComponent } from './shop-suppliers.component';
import { ShopSuppliersRoutingModule } from './shop-suppliers-routing.module';
import { ShopSupplierEditorComponent } from './shop-supplier-editor.component';

@NgModule({
  declarations: [ShopSuppliersComponent, ShopSupplierEditorComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopSuppliersRoutingModule],
})
export class ShopSuppliersModule {}
