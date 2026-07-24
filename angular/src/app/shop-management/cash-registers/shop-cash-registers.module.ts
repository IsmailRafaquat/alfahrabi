import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopCashRegistersComponent } from './shop-cash-registers.component';
import { ShopCashRegistersRoutingModule } from './shop-cash-registers-routing.module';

@NgModule({
  declarations: [ShopCashRegistersComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopCashRegistersRoutingModule],
})
export class ShopCashRegistersModule {}
