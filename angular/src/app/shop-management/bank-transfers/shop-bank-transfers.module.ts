import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopBankTransfersComponent } from './shop-bank-transfers.component';
import { ShopBankTransferEditorComponent } from './shop-bank-transfer-editor.component';
import { ShopBankTransferDetailComponent } from './shop-bank-transfer-detail.component';
import { ShopBankTransfersRoutingModule } from './shop-bank-transfers-routing.module';

@NgModule({
  declarations: [ShopBankTransfersComponent, ShopBankTransferEditorComponent, ShopBankTransferDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopBankTransfersRoutingModule],
})
export class ShopBankTransfersModule {}
