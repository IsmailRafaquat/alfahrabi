import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../../shared/shared.module';
import { ShopAiAssistantComponent } from './shop-ai-assistant.component';
import { ShopAiAssistantRoutingModule } from './shop-ai-assistant-routing.module';

@NgModule({
  declarations: [ShopAiAssistantComponent],
  imports: [CommonModule, FormsModule, SharedModule, PageModule, ShopAiAssistantRoutingModule],
})
export class ShopAiAssistantModule {}
