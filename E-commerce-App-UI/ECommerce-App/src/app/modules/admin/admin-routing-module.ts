import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Dashboard } from './dashboard/dashboard';
import { MainLayout } from './dashboard-layout/main-layout/main-layout';
import { AllProduct } from './all-product/all-product';
import { AddProduct } from './add-product/add-product';
import { Suppliers } from './suppliers/suppliers';
import { User } from './user/user';
import { Roles } from './roles/roles';
import { Category } from './category/category';
import { Orders } from './orders/orders';
import { RoleAdd } from './role-add/role-add';
import { CategoryAdd } from './category-add/category-add';
import { SupplierAdd } from './supplier-add/supplier-add';
import { Settings } from './settings/settings';

const routes: Routes = [
 
  {
    path:'',
    component:MainLayout,
    children:[
        {path:'dashboard',component:Dashboard},
        {path:'allproduct',component:AllProduct},
        {path:'addproduct',component:AddProduct},
        {path:'allsuppliers',component:Suppliers},
        {path:'supplier-add',component:SupplierAdd},
        {path:'users',component:User},
        {path:'roles',component:Roles},
        {path:'role-add',component:RoleAdd},
        {path:'category',component:Category},
        {path:'category-add',component:CategoryAdd},
        {path:'orders', component:Orders},
        {path:'settings', component:Settings}
    ]
  }
];


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
