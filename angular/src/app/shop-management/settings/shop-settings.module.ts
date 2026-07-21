import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '../../shared/shared.module';
import { ShopSettingsRoutingModule } from './shop-settings-routing.module';
import { ShopSettingsComponent } from './shop-settings.component';
@NgModule({ declarations: [ShopSettingsComponent], imports: [CommonModule, SharedModule, ShopSettingsRoutingModule] })
export class ShopSettingsModule {}
