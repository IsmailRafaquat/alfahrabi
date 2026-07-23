import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopCustomerPaymentsComponent } from './shop-customer-payments.component';
import { ShopCustomerPaymentsRoutingModule } from './shop-customer-payments-routing.module';
import { ShopCustomerPaymentEditorComponent } from './shop-customer-payment-editor.component';
import { ShopCustomerPaymentDetailComponent } from './shop-customer-payment-detail.component';

@NgModule({
  declarations: [ShopCustomerPaymentsComponent, ShopCustomerPaymentEditorComponent, ShopCustomerPaymentDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopCustomerPaymentsRoutingModule],
})
export class ShopCustomerPaymentsModule {}
