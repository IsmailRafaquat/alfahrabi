import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopStockCountsComponent } from './shop-stock-counts.component';
import { ShopStockCountsRoutingModule } from './shop-stock-counts-routing.module';
import { ShopStockCountEditorComponent } from './shop-stock-count-editor.component';
import { ShopStockCountCountingComponent } from './shop-stock-count-counting.component';
import { ShopStockCountDetailComponent } from './shop-stock-count-detail.component';

@NgModule({
  declarations: [ShopStockCountsComponent, ShopStockCountEditorComponent, ShopStockCountCountingComponent, ShopStockCountDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopStockCountsRoutingModule],
})
export class ShopStockCountsModule {}
