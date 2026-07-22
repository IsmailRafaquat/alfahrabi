import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopSupplierLedgerComponent } from './shop-supplier-ledger.component';
import { ShopSupplierLedgerRoutingModule } from './shop-supplier-ledger-routing.module';

@NgModule({
  declarations: [ShopSupplierLedgerComponent],
  imports: [CommonModule, FormsModule, SharedModule, ShopSupplierLedgerRoutingModule],
})
export class ShopSupplierLedgerModule {}
