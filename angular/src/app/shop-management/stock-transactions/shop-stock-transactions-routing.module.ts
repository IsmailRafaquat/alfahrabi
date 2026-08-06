import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopStockTransactionsComponent } from './shop-stock-transactions.component';

const routes: Routes = [
  { path: '', component: ShopStockTransactionsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.StockTransactions' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopStockTransactionsRoutingModule {}
