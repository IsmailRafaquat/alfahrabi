import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopBankTransactionsComponent } from './shop-bank-transactions.component';

const routes: Routes = [
  { path: '', component: ShopBankTransactionsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.BankTransactions' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopBankTransactionsRoutingModule {}
