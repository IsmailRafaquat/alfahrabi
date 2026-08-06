import { inject, provideAppInitializer } from '@angular/core';
import { NavItemsService } from '@abp/ng.theme.shared';
import { ShopNotificationBellComponent } from './shop-notification-bell.component';

/**
 * Registers the Shop Notification bell into the Lepton-X navbar via the same NavItemsService the
 * theme itself uses for the language switcher and user-profile icon (see
 * node_modules/@abp/ng.theme.lepton-x nav-item.provider.ts) - no theme replacement/fork needed.
 * `requiredPolicy` makes the *abpPermission directive inside NavItemsComponent hide it entirely for
 * users without ShopManagement.Notifications.View.
 */
export const NOTIFICATION_BELL_PROVIDER = provideAppInitializer(() => {
  const navItems = inject(NavItemsService);

  navItems.addItems([
    {
      id: 'ShopNotificationBell',
      order: 50,
      component: ShopNotificationBellComponent,
      requiredPolicy: 'ShopManagement.Notifications.View',
    },
  ]);
});
