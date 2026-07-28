import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ShopDashboardComponent } from './shop-dashboard.component';
import { ShopDashboardRoutingModule } from './shop-dashboard-routing.module';

@NgModule({
  declarations: [ShopDashboardComponent],
  imports: [CommonModule, FormsModule, SharedModule, ShopDashboardRoutingModule],
})
export class ShopDashboardModule {}
