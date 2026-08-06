import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopSupplierLedgerComponent } from './shop-supplier-ledger.component';

const routes: Routes = [
  { path: '', component: ShopSupplierLedgerComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.SupplierLedger' } },
  { path: ':supplierId', component: ShopSupplierLedgerComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.SupplierLedger' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopSupplierLedgerRoutingModule {}
