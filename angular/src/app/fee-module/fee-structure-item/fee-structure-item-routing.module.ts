import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FeeStructureItemComponent } from './fee-structure-item.component';

const routes: Routes = [{ path: '', component: FeeStructureItemComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FeeStructureItemRoutingModule { }
