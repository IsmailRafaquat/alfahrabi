import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopBankTransactionsComponent } from './shop-bank-transactions.component';
import { ShopBankTransactionsRoutingModule } from './shop-bank-transactions-routing.module';

@NgModule({
  declarations: [ShopBankTransactionsComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopBankTransactionsRoutingModule],
})
export class ShopBankTransactionsModule {}
