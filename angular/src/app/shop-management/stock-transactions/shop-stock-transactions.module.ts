import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopStockTransactionsComponent } from './shop-stock-transactions.component';
import { ShopStockTransactionsRoutingModule } from './shop-stock-transactions-routing.module';

@NgModule({
  declarations: [ShopStockTransactionsComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopStockTransactionsRoutingModule],
})
export class ShopStockTransactionsModule {}
