import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopBankTransfersComponent } from './shop-bank-transfers.component';
import { ShopBankTransferEditorComponent } from './shop-bank-transfer-editor.component';
import { ShopBankTransferDetailComponent } from './shop-bank-transfer-detail.component';

const routes: Routes = [
  { path: '', component: ShopBankTransfersComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.BankTransfers' } },
  { path: 'create', component: ShopBankTransferEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.BankTransfers.Create' } },
  { path: ':id/edit', component: ShopBankTransferEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.BankTransfers.Edit' } },
  { path: ':id', component: ShopBankTransferDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.BankTransfers' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopBankTransfersRoutingModule {}
