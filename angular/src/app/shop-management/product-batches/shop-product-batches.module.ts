import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopProductBatchesComponent } from './shop-product-batches.component';
import { ShopProductBatchesRoutingModule } from './shop-product-batches-routing.module';
import { ShopProductBatchDetailComponent } from './shop-product-batch-detail.component';

@NgModule({
  declarations: [ShopProductBatchesComponent, ShopProductBatchDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopProductBatchesRoutingModule],
})
export class ShopProductBatchesModule {}
