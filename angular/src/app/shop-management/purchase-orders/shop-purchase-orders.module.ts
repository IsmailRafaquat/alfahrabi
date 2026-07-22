import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopPurchaseOrdersComponent } from './shop-purchase-orders.component';
import { ShopPurchaseOrdersRoutingModule } from './shop-purchase-orders-routing.module';
import { ShopPurchaseOrderEditorComponent } from './shop-purchase-order-editor.component';
import { ShopPurchaseOrderDetailComponent } from './shop-purchase-order-detail.component';

@NgModule({
  declarations: [ShopPurchaseOrdersComponent, ShopPurchaseOrderEditorComponent, ShopPurchaseOrderDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopPurchaseOrdersRoutingModule],
})
export class ShopPurchaseOrdersModule {}
