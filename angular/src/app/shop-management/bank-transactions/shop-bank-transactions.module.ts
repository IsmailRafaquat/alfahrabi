import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopBankTransactionsComponent } from './shop-bank-transactions.component';
import { ShopBankTransactionsRoutingModule } from './shop-bank-transactions-routing.module';

@NgModule({
  declarations: [ShopBankTransactionsComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopBankTransactionsRoutingModule],
})
export class ShopBankTransactionsModule {}
