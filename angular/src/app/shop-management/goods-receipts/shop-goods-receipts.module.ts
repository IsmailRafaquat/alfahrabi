import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopGoodsReceiptsComponent } from './shop-goods-receipts.component';
import { ShopGoodsReceiptsRoutingModule } from './shop-goods-receipts-routing.module';
import { ShopGoodsReceiptEditorComponent } from './shop-goods-receipt-editor.component';
import { ShopGoodsReceiptDetailComponent } from './shop-goods-receipt-detail.component';

@NgModule({
  declarations: [ShopGoodsReceiptsComponent, ShopGoodsReceiptEditorComponent, ShopGoodsReceiptDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopGoodsReceiptsRoutingModule],
})
export class ShopGoodsReceiptsModule {}
