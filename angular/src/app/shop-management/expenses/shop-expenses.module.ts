import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopExpensesComponent } from './shop-expenses.component';
import { ShopExpensesRoutingModule } from './shop-expenses-routing.module';
import { ShopExpenseEditorComponent } from './shop-expense-editor.component';
import { ShopExpenseDetailComponent } from './shop-expense-detail.component';

@NgModule({
  declarations: [ShopExpensesComponent, ShopExpenseEditorComponent, ShopExpenseDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopExpensesRoutingModule],
})
export class ShopExpensesModule {}
