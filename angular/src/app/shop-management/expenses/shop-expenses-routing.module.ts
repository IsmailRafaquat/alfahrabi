import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopExpensesComponent } from './shop-expenses.component';
import { ShopExpenseEditorComponent } from './shop-expense-editor.component';
import { ShopExpenseDetailComponent } from './shop-expense-detail.component';

const routes: Routes = [
  { path: '', component: ShopExpensesComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Expenses' } },
  { path: 'create', component: ShopExpenseEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Expenses.Create' } },
  { path: ':id/edit', component: ShopExpenseEditorComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Expenses.Edit' } },
  { path: ':id', component: ShopExpenseDetailComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Expenses' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopExpensesRoutingModule {}
