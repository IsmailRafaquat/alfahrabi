import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopStockAdjustmentsComponent } from './shop-stock-adjustments.component';
import { ShopStockAdjustmentsRoutingModule } from './shop-stock-adjustments-routing.module';
import { ShopStockAdjustmentEditorComponent } from './shop-stock-adjustment-editor.component';
import { ShopStockAdjustmentDetailComponent } from './shop-stock-adjustment-detail.component';

@NgModule({
  declarations: [ShopStockAdjustmentsComponent, ShopStockAdjustmentEditorComponent, ShopStockAdjustmentDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopStockAdjustmentsRoutingModule],
})
export class ShopStockAdjustmentsModule {}
