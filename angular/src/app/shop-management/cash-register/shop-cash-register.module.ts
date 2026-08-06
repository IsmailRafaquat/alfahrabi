import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopCashRegisterComponent } from './shop-cash-register.component';
import { ShopCashTransactionsComponent } from './shop-cash-transactions.component';
import { ShopCashClosingsComponent } from './shop-cash-closings.component';
import { ShopCashClosingDetailComponent } from './shop-cash-closing-detail.component';
import { ShopCashRegisterRoutingModule } from './shop-cash-register-routing.module';

@NgModule({
  declarations: [
    ShopCashRegisterComponent,
    ShopCashTransactionsComponent,
    ShopCashClosingsComponent,
    ShopCashClosingDetailComponent,
  ],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopCashRegisterRoutingModule],
})
export class ShopCashRegisterModule {}
