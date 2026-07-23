import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopSalesComponent } from './shop-sales.component';
import { ShopSalesRoutingModule } from './shop-sales-routing.module';
import { ShopSaleEditorComponent } from './shop-sale-editor.component';
import { ShopSaleDetailComponent } from './shop-sale-detail.component';

@NgModule({
  declarations: [ShopSalesComponent, ShopSaleEditorComponent, ShopSaleDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopSalesRoutingModule],
})
export class ShopSalesModule {}
