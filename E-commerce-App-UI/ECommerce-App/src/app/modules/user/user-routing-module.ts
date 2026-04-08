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
import { ContactUs } from './contact-us/contact-us';
import { Faqs } from './faqs/faqs';
import { ShippingInfo } from './shipping-info/shipping-info';
import { ReturnsRefunds } from './returns-refunds/returns-refunds';
import { PrivacyPolicy } from './privacy-policy/privacy-policy';
import { TermsOfService } from './terms-of-service/terms-of-service';
import { CookiePolicy } from './cookie-policy/cookie-policy';

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
  {path:'contact-us',component:ContactUs},
  {path:'faqs',component:Faqs},
  {path:'shipping-info',component:ShippingInfo},
  {path:'returns-refunds',component:ReturnsRefunds},
  {path:'privacy-policy',component:PrivacyPolicy},
  {path:'terms-of-service',component:TermsOfService},
  {path:'cookie-policy',component:CookiePolicy},
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserRoutingModule { }
