import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LateFeePolicyComponent } from './late-fee-policy.component';

const routes: Routes = [{ path: '', component: LateFeePolicyComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LateFeePolicyRoutingModule { }
