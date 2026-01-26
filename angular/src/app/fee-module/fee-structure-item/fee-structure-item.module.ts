import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FeeStructureItemRoutingModule } from './fee-structure-item-routing.module';
import { FeeStructureItemComponent } from './fee-structure-item.component';
import { SharedModule } from 'src/app/shared/shared.module';


@NgModule({
  declarations: [
    FeeStructureItemComponent
  ],
  imports: [
    CommonModule,
    FeeStructureItemRoutingModule,
    SharedModule
  ]
})
export class FeeStructureItemModule { }
