import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopGoodsReceiptsComponent } from './shop-goods-receipts.component';
import { ShopGoodsReceiptsRoutingModule } from './shop-goods-receipts-routing.module';
import { ShopGoodsReceiptEditorComponent } from './shop-goods-receipt-editor.component';
import { ShopGoodsReceiptDetailComponent } from './shop-goods-receipt-detail.component';

@NgModule({
  declarations: [ShopGoodsReceiptsComponent, ShopGoodsReceiptEditorComponent, ShopGoodsReceiptDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopGoodsReceiptsRoutingModule],
})
export class ShopGoodsReceiptsModule {}
