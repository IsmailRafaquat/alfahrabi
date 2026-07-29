import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ShopDashboardComponent } from './shop-dashboard.component';
import { ShopDashboardRoutingModule } from './shop-dashboard-routing.module';

@NgModule({
  declarations: [ShopDashboardComponent],
  imports: [CommonModule, FormsModule, SharedModule, PageModule, TopbarLayoutModule, ShopDashboardRoutingModule],
})
export class ShopDashboardModule {}
