import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { LateFeePolicyRoutingModule } from './late-fee-policy-routing.module';
import { LateFeePolicyComponent } from './late-fee-policy.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    LateFeePolicyComponent
  ],
  imports: [
    CommonModule,
    LateFeePolicyRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class LateFeePolicyModule { }
