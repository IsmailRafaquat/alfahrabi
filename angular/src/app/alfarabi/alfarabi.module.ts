import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AlfarabiRoutingModule } from './alfarabi-routing.module';
import { AlfarabiComponent } from './alfarabi.component';


@NgModule({
  declarations: [
    AlfarabiComponent
  ],
  imports: [
    CommonModule,
    AlfarabiRoutingModule
  ]
})
export class AlfarabiModule { }
