import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopStockTransactionsComponent } from './shop-stock-transactions.component';
import { ShopStockTransactionsRoutingModule } from './shop-stock-transactions-routing.module';

@NgModule({
  declarations: [ShopStockTransactionsComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopStockTransactionsRoutingModule],
})
export class ShopStockTransactionsModule {}
