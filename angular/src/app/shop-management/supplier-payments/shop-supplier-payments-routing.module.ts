import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopSupplierPaymentsComponent } from './shop-supplier-payments.component';
import { ShopSupplierPaymentEditorComponent } from './shop-supplier-payment-editor.component';
import { ShopSupplierPaymentDetailComponent } from './shop-supplier-payment-detail.component';

const routes: Routes = [
  { path: '', component: ShopSupplierPaymentsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.SupplierPayments' } },
  { path: 'create', component: ShopSupplierPaymentEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.SupplierPayments.Create' } },
  { path: ':id/edit', component: ShopSupplierPaymentEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.SupplierPayments.Edit' } },
  { path: ':id', component: ShopSupplierPaymentDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.SupplierPayments' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopSupplierPaymentsRoutingModule {}
