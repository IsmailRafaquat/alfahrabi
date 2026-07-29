import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TopbarLayoutComponent } from './topbar-layout.component';
import { SharedModule } from 'src/app/shared/shared.module';


@NgModule({
  declarations: [
    TopbarLayoutComponent
  ],
  imports: [
    CommonModule,
    SharedModule
  ],
  exports: [
    TopbarLayoutComponent
  ]
})
export class TopbarLayoutModule { }
