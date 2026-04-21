import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TopbarLayoutComponent } from './topbar-layout.component';

const routes: Routes = [{ path: '', component: TopbarLayoutComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TopbarLayoutRoutingModule { }
