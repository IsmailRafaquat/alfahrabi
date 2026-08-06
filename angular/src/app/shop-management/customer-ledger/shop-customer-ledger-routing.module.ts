import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopCustomerLedgerComponent } from './shop-customer-ledger.component';

const routes: Routes = [
  { path: '', component: ShopCustomerLedgerComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CustomerLedger' } },
  { path: ':customerId', component: ShopCustomerLedgerComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CustomerLedger' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopCustomerLedgerRoutingModule {}
