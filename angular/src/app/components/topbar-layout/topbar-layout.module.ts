import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TopbarLayoutRoutingModule } from './topbar-layout-routing.module';
import { TopbarLayoutComponent } from './topbar-layout.component';
import { SharedModule } from 'src/app/shared/shared.module';


@NgModule({
  declarations: [
    TopbarLayoutComponent
  ],
  imports: [
    CommonModule,
    TopbarLayoutRoutingModule,
    SharedModule
  ],
  exports: [
    TopbarLayoutComponent
  ]
})
export class TopbarLayoutModule { }
