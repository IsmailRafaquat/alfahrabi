import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopCustomerLedgerComponent } from './shop-customer-ledger.component';
import { ShopCustomerLedgerRoutingModule } from './shop-customer-ledger-routing.module';

@NgModule({
  declarations: [ShopCustomerLedgerComponent],
  imports: [CommonModule, FormsModule, SharedModule, ShopCustomerLedgerRoutingModule],
})
export class ShopCustomerLedgerModule {}
