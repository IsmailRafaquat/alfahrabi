import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ExpenseEntryRoutingModule } from './expense-entry-routing.module';
import { ExpenseEntryComponent } from './expense-entry.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { PageModule } from '@abp/ng.components/page';


@NgModule({
  declarations: [
    ExpenseEntryComponent
  ],
  imports: [
    CommonModule,
    ExpenseEntryRoutingModule,
    SharedModule,
    PageModule
  ]
})
export class ExpenseEntryModule { }
