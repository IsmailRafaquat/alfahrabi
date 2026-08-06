import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopCashRegisterComponent } from './shop-cash-register.component';
import { ShopCashTransactionsComponent } from './shop-cash-transactions.component';
import { ShopCashClosingsComponent } from './shop-cash-closings.component';
import { ShopCashClosingDetailComponent } from './shop-cash-closing-detail.component';

const routes: Routes = [
  { path: '', component: ShopCashRegisterComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CashRegisters' } },
  { path: 'transactions', component: ShopCashTransactionsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CashTransactions' } },
  { path: 'closings', component: ShopCashClosingsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CashClosings' } },
  { path: 'closings/:id', component: ShopCashClosingDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CashClosings' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopCashRegisterRoutingModule {}
