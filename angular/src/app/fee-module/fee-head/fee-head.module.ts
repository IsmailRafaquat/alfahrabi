import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FeeHeadRoutingModule } from './fee-head-routing.module';
import { FeeHeadComponent } from './fee-head.component';
import { SharedModule } from 'src/app/shared/shared.module';


@NgModule({
  declarations: [
    FeeHeadComponent
  ],
  imports: [
    CommonModule,
    FeeHeadRoutingModule,
    SharedModule
  ]
})
export class FeeHeadModule { }
