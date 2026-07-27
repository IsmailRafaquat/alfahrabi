import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopProductBatchesComponent } from './shop-product-batches.component';
import { ShopProductBatchesRoutingModule } from './shop-product-batches-routing.module';
import { ShopProductBatchDetailComponent } from './shop-product-batch-detail.component';

@NgModule({
  declarations: [ShopProductBatchesComponent, ShopProductBatchDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopProductBatchesRoutingModule],
})
export class ShopProductBatchesModule {}
