import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreModule } from '@abp/ng.core';
import { ShopNotificationBellComponent } from './shop-notification-bell.component';

@NgModule({
  declarations: [ShopNotificationBellComponent],
  imports: [CommonModule, CoreModule],
  exports: [ShopNotificationBellComponent],
})
export class NotificationBellModule {}
