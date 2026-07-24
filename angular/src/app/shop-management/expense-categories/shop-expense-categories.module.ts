import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopExpenseCategoriesComponent } from './shop-expense-categories.component';
import { ShopExpenseCategoriesRoutingModule } from './shop-expense-categories-routing.module';

@NgModule({
  declarations: [ShopExpenseCategoriesComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopExpenseCategoriesRoutingModule],
})
export class ShopExpenseCategoriesModule {}
