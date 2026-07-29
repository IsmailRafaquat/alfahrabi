import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopSalesComponent } from './shop-sales.component';
import { ShopSalesRoutingModule } from './shop-sales-routing.module';
import { ShopSaleEditorComponent } from './shop-sale-editor.component';
import { ShopSaleDetailComponent } from './shop-sale-detail.component';

@NgModule({
  declarations: [ShopSalesComponent, ShopSaleEditorComponent, ShopSaleDetailComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopSalesRoutingModule],
})
export class ShopSalesModule {}
