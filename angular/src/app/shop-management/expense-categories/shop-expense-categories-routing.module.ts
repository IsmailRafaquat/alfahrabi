import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopExpenseCategoriesComponent } from './shop-expense-categories.component';

const routes: Routes = [
  { path: '', component: ShopExpenseCategoriesComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.ExpenseCategories' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopExpenseCategoriesRoutingModule {}
