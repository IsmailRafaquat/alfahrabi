import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopSupplierPaymentsComponent } from './shop-supplier-payments.component';
import { ShopSupplierPaymentsRoutingModule } from './shop-supplier-payments-routing.module';
import { ShopSupplierPaymentEditorComponent } from './shop-supplier-payment-editor.component';
import { ShopSupplierPaymentDetailComponent } from './shop-supplier-payment-detail.component';

@NgModule({
  declarations: [ShopSupplierPaymentsComponent, ShopSupplierPaymentEditorComponent, ShopSupplierPaymentDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopSupplierPaymentsRoutingModule],
})
export class ShopSupplierPaymentsModule {}
