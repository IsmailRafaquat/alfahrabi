import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopBankAccountsComponent } from './shop-bank-accounts.component';
import { ShopBankAccountEditorComponent } from './shop-bank-account-editor.component';
import { ShopBankAccountDetailComponent } from './shop-bank-account-detail.component';
import { ShopBankAccountsRoutingModule } from './shop-bank-accounts-routing.module';

@NgModule({
  declarations: [ShopBankAccountsComponent, ShopBankAccountEditorComponent, ShopBankAccountDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopBankAccountsRoutingModule],
})
export class ShopBankAccountsModule {}
