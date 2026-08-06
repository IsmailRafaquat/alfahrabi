import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopCustomerPaymentsComponent } from './shop-customer-payments.component';
import { ShopCustomerPaymentEditorComponent } from './shop-customer-payment-editor.component';
import { ShopCustomerPaymentDetailComponent } from './shop-customer-payment-detail.component';

const routes: Routes = [
  { path: '', component: ShopCustomerPaymentsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CustomerPayments' } },
  { path: 'create', component: ShopCustomerPaymentEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CustomerPayments.Create' } },
  { path: ':id/edit', component: ShopCustomerPaymentEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CustomerPayments.Edit' } },
  { path: ':id', component: ShopCustomerPaymentDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CustomerPayments' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopCustomerPaymentsRoutingModule {}
