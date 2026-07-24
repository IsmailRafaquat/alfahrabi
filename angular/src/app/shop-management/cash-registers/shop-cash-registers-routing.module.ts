import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopCashRegistersComponent } from './shop-cash-registers.component';

const routes: Routes = [
  { path: '', component: ShopCashRegistersComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.CashRegisters' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopCashRegistersRoutingModule {}
