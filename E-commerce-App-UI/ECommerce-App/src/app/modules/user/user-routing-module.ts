import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Homepage1 } from './homepage1/homepage1';
import { ProductList } from './product-list/product-list';
import { Cart } from './cart/cart';
import { ProductDetails } from './product-details/product-details';
import { Checkout } from './checkout/checkout';
import { PaymentSuccess } from './payment-success/payment-success';
import { PaymentFailure } from './payment-failure/payment-failure';
import { Orders } from './orders/orders';
import { Profile } from './profile/profile';

const routes: Routes = [
  {path:'',component:Homepage1},
  {path:'homepage', component:Homepage1},
  {path:'product-list',component:ProductList},
  {path:'cart',component:Cart},
  { path: 'product/:id', component: ProductDetails},
  {path:'checkout',component:Checkout},
  {path:'payment-success',component:PaymentSuccess},
  {path:'payment-failure',component:PaymentFailure},
  {path:'orders',component:Orders},
  {path:'profile',component:Profile},
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserRoutingModule { }
