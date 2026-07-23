import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopCustomersComponent } from './shop-customers.component';
import { ShopCustomersRoutingModule } from './shop-customers-routing.module';
import { ShopCustomerEditorComponent } from './shop-customer-editor.component';

@NgModule({
  declarations: [ShopCustomersComponent, ShopCustomerEditorComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopCustomersRoutingModule],
})
export class ShopCustomersModule {}
