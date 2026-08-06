import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopCashRegistersComponent } from './shop-cash-registers.component';
import { ShopCashRegistersRoutingModule } from './shop-cash-registers-routing.module';

@NgModule({
  declarations: [ShopCashRegistersComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopCashRegistersRoutingModule],
})
export class ShopCashRegistersModule {}
