import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CreateStudentRoutingModule } from './create-student-routing.module';
import { CreateStudentComponent } from './create-student.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    CreateStudentComponent
  ],
  imports: [
    CommonModule,
    CreateStudentRoutingModule,
    SharedModule,
    PageModule,
  ]
})
export class CreateStudentModule { }
