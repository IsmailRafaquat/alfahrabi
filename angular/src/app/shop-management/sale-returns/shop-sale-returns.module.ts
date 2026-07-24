import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopSaleReturnsComponent } from './shop-sale-returns.component';
import { ShopSaleReturnsRoutingModule } from './shop-sale-returns-routing.module';
import { ShopSaleReturnEditorComponent } from './shop-sale-return-editor.component';
import { ShopSaleReturnDetailComponent } from './shop-sale-return-detail.component';

@NgModule({
  declarations: [ShopSaleReturnsComponent, ShopSaleReturnEditorComponent, ShopSaleReturnDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopSaleReturnsRoutingModule],
})
export class ShopSaleReturnsModule {}
