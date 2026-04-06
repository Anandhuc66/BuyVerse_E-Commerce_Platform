import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SupplierRoutingModule } from './supplier-routing-module';
import { SupplierRegister } from './supplier-register/supplier-register';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Header } from './dashboard-layout/header/header';
import { SideBar } from './dashboard-layout/side-bar/side-bar';
import { MainLayout } from './dashboard-layout/main-layout/main-layout';
import { ProductAdd } from './product-add/product-add';
import { Dashboard } from './dashboard/dashboard';
import { Products } from './products/products';
import { AgGridAngular } from 'ag-grid-angular';
import { SupplierOrders } from './orders/orders';
import { BaseChartDirective } from 'ng2-charts';
import { SupplierSettings } from './settings/settings';


@NgModule({
  declarations: [
    SupplierRegister,
    Header,
    SideBar,
    MainLayout,
    ProductAdd,
    Dashboard,
    Products,
    SupplierOrders,
    SupplierSettings,
  ],
  imports: [
    CommonModule,
    SupplierRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    AgGridAngular,
    BaseChartDirective
  ]
})
export class SupplierModule { }
