import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FeeHeadComponent } from './fee-head.component';

const routes: Routes = [{ path: '', component: FeeHeadComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FeeHeadRoutingModule { }
