import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopAiAssistantComponent } from './shop-ai-assistant.component';

const routes: Routes = [
  { path: '', component: ShopAiAssistantComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.AiAssistant' } },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopAiAssistantRoutingModule {}
