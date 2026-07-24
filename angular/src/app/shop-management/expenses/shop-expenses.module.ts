import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopExpensesComponent } from './shop-expenses.component';
import { ShopExpensesRoutingModule } from './shop-expenses-routing.module';
import { ShopExpenseEditorComponent } from './shop-expense-editor.component';
import { ShopExpenseDetailComponent } from './shop-expense-detail.component';

@NgModule({
  declarations: [ShopExpensesComponent, ShopExpenseEditorComponent, ShopExpenseDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopExpensesRoutingModule],
})
export class ShopExpensesModule {}
