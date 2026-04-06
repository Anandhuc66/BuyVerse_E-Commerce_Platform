import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { CoreRoutingModule } from './core-routing-module';

import { Header } from './common/header/header';
import { Footer } from './common/footer/footer';
import { CatNavbar } from './common/cat-navbar/cat-navbar';

import { FormsModule } from '@angular/forms';



@NgModule({
  declarations: [
    Header,
    Footer,
    CatNavbar
  ],
  imports: [
    CommonModule,
    CoreRoutingModule,
    RouterModule,
    FormsModule
  ],
  exports: [
    Header,
    Footer,
    CatNavbar
  ],
  providers: [
  ]
})
export class CoreModule { }
