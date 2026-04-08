import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UserRoutingModule } from './user-routing-module';
import { Homepage1 } from './homepage1/homepage1';
import { ProductList } from './product-list/product-list';
import { ProductDetails } from './product-details/product-details';
import { Cart } from './cart/cart';
import { Checkout } from './checkout/checkout';
import { Orders } from './orders/orders';
import { PaymentSuccess } from './payment-success/payment-success';
import { PaymentFailure } from './payment-failure/payment-failure';
import { Profile } from './profile/profile';
import { ContactUs } from './contact-us/contact-us';
import { Faqs } from './faqs/faqs';
import { ShippingInfo } from './shipping-info/shipping-info';
import { ReturnsRefunds } from './returns-refunds/returns-refunds';
import { PrivacyPolicy } from './privacy-policy/privacy-policy';
import { TermsOfService } from './terms-of-service/terms-of-service';
import { CookiePolicy } from './cookie-policy/cookie-policy';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';


@NgModule({
  declarations: [
    Homepage1,
    ProductList,
    ProductDetails,
    Cart,
    Checkout,
    Orders,
    PaymentSuccess,
    PaymentFailure,
    Profile,
    ContactUs,
    Faqs,
    ShippingInfo,
    ReturnsRefunds,
    PrivacyPolicy,
    TermsOfService,
    CookiePolicy
  ],
  imports: [
    CommonModule,
    UserRoutingModule,
    FormsModule,
    ReactiveFormsModule
  ]
})
export class UserModule { }
