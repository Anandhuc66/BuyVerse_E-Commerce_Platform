import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SupplierRegister } from './supplier-register/supplier-register';
import { MainLayout } from './dashboard-layout/main-layout/main-layout';
import { ProductAdd } from './product-add/product-add';
import { Dashboard } from './dashboard/dashboard';
import { Products } from './products/products';
import { SupplierOrders } from './orders/orders';
import { SupplierSettings } from './settings/settings';

const routes: Routes = [
  {
      path:'',
      component:MainLayout,
      children:[
        {path:'dashboard',component:Dashboard},
        {path:'product-add',component:ProductAdd},
        {path:'products',component:Products},
        {path:'orders',component:SupplierOrders},
        {path:'settings',component:SupplierSettings}
      ]
    },
  {path:'supplier-register', component:SupplierRegister},

  
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SupplierRoutingModule { }
