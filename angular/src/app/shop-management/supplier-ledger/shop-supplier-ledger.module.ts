import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopSupplierLedgerComponent } from './shop-supplier-ledger.component';
import { ShopSupplierLedgerRoutingModule } from './shop-supplier-ledger-routing.module';

@NgModule({
  declarations: [ShopSupplierLedgerComponent],
  imports: [CommonModule, FormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopSupplierLedgerRoutingModule],
})
export class ShopSupplierLedgerModule {}
