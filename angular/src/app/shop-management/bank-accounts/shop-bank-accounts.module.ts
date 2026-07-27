import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopBankAccountsComponent } from './shop-bank-accounts.component';
import { ShopBankAccountEditorComponent } from './shop-bank-account-editor.component';
import { ShopBankAccountDetailComponent } from './shop-bank-account-detail.component';
import { ShopBankAccountsRoutingModule } from './shop-bank-accounts-routing.module';

@NgModule({
  declarations: [ShopBankAccountsComponent, ShopBankAccountEditorComponent, ShopBankAccountDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ShopBankAccountsRoutingModule],
})
export class ShopBankAccountsModule {}
