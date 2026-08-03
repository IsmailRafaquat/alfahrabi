import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ShopNotificationCenterComponent } from './shop-notification-center.component';

const routes: Routes = [{ path: '', component: ShopNotificationCenterComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopNotificationsRoutingModule {}
