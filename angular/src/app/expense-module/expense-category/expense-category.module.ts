import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ExpenseCategoryRoutingModule } from './expense-category-routing.module';
import { ExpenseCategoryComponent } from './expense-category.component';
import { SharedModule } from 'src/app/shared/shared.module';


@NgModule({
  declarations: [
    ExpenseCategoryComponent
  ],
  imports: [
    CommonModule,
    ExpenseCategoryRoutingModule,
    SharedModule
  ]
})
export class ExpenseCategoryModule { }
