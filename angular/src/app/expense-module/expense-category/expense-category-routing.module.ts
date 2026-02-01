import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ExpenseCategoryComponent } from './expense-category.component';

const routes: Routes = [{ path: '', component: ExpenseCategoryComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ExpenseCategoryRoutingModule { }
