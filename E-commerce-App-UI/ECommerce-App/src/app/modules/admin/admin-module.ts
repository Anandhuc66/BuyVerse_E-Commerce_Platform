import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdminRoutingModule } from './admin-routing-module';
import { Dashboard } from './dashboard/dashboard';
import { Header } from './dashboard-layout/header/header';
import { SideBar } from './dashboard-layout/side-bar/side-bar';
import { MainLayout } from './dashboard-layout/main-layout/main-layout';
import { AllProduct } from './all-product/all-product';
import { AddProduct } from './add-product/add-product';
import { AgGridAngular } from 'ag-grid-angular';
import { Suppliers } from './suppliers/suppliers';
import { User } from './user/user';
import { Category } from './category/category';
import { Roles } from './roles/roles';
import { Orders } from './orders/orders';
import { RoleAdd } from './role-add/role-add';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CategoryAdd } from './category-add/category-add';
import { SupplierAdd } from './supplier-add/supplier-add';
import { BaseChartDirective } from 'ng2-charts';
import { Settings } from './settings/settings';



@NgModule({
  declarations: [
    Dashboard,
    Header,
    SideBar,
    MainLayout,
    AllProduct,
    AddProduct,
    Suppliers,
    User,
    Category,
    Roles,
    Orders,
    RoleAdd,
    CategoryAdd,
    SupplierAdd,
    Settings,

  ],
  imports: [
    CommonModule,
    AdminRoutingModule,
    AgGridAngular,
    ReactiveFormsModule,
    FormsModule,
    BaseChartDirective
    
  ]
})
export class AdminModule { }
