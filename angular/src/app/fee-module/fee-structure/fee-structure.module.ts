import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FeeStructureRoutingModule } from './fee-structure-routing.module';
import { FeeStructureComponent } from './fee-structure.component';
import { SharedModule } from 'src/app/shared/shared.module';


@NgModule({
  declarations: [
    FeeStructureComponent
  ],
  imports: [
    CommonModule,
    FeeStructureRoutingModule,
    SharedModule
  ]
})
export class FeeStructureModule { }
