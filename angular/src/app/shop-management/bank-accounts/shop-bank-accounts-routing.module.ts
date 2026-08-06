import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopBankAccountsComponent } from './shop-bank-accounts.component';
import { ShopBankAccountEditorComponent } from './shop-bank-account-editor.component';
import { ShopBankAccountDetailComponent } from './shop-bank-account-detail.component';

const routes: Routes = [
  { path: '', component: ShopBankAccountsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.BankAccounts' } },
  { path: 'create', component: ShopBankAccountEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.BankAccounts.Create' } },
  { path: ':id/edit', component: ShopBankAccountEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.BankAccounts.Edit' } },
  { path: ':id', component: ShopBankAccountDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.BankAccounts' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopBankAccountsRoutingModule {}
