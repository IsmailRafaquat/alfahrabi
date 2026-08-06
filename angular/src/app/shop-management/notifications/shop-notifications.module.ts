import { NgModule } from '@angular/core';
import { SharedReportsModule } from '../../shared/reports/shared-reports.module';
import { ShopNotificationCenterComponent } from './shop-notification-center.component';
import { ShopNotificationsRoutingModule } from './shop-notifications-routing.module';

@NgModule({
  declarations: [ShopNotificationCenterComponent],
  imports: [SharedReportsModule, ShopNotificationsRoutingModule],
})
export class ShopNotificationsModule {}
