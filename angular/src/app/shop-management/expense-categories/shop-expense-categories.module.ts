import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopExpenseCategoriesComponent } from './shop-expense-categories.component';
import { ShopExpenseCategoriesRoutingModule } from './shop-expense-categories-routing.module';

@NgModule({
  declarations: [ShopExpenseCategoriesComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopExpenseCategoriesRoutingModule],
})
export class ShopExpenseCategoriesModule {}
