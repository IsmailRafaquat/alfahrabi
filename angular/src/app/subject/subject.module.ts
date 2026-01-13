import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SubjectRoutingModule } from './subject-routing.module';
import { SubjectComponent } from './subject.component';
import { SharedModule } from '../shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    SubjectComponent
  ],
  imports: [
    CommonModule,
    SubjectRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class SubjectModule { }
