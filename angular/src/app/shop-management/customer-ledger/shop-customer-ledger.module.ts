import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopCustomerLedgerComponent } from './shop-customer-ledger.component';
import { ShopCustomerLedgerRoutingModule } from './shop-customer-ledger-routing.module';

@NgModule({
  declarations: [ShopCustomerLedgerComponent],
  imports: [CommonModule, FormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopCustomerLedgerRoutingModule],
})
export class ShopCustomerLedgerModule {}
