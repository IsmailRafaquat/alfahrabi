import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { ShopReportsComponent } from './shop-reports.component';
import { ShopReportsRoutingModule } from './shop-reports-routing.module';

@NgModule({
  declarations: [ShopReportsComponent],
  imports: [CommonModule, SharedModule, PageModule, ShopReportsRoutingModule],
})
export class ShopReportsModule {}
