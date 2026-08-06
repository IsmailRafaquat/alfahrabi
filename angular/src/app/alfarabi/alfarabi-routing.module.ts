import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AlfarabiComponent } from './alfarabi.component';

const routes: Routes = [{ path: '', component: AlfarabiComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AlfarabiRoutingModule { }
