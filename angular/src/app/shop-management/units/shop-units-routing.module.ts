import { NgModule } from '@angular/core'; import { RouterModule, Routes } from '@angular/router'; import { permissionGuard } from '@abp/ng.core'; import { ShopUnitsComponent } from './shop-units.component';
const routes: Routes = [{ path: '', component: ShopUnitsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Units' } }];
@NgModule({ imports:[RouterModule.forChild(routes)], exports:[RouterModule] }) export class ShopUnitsRoutingModule {}
