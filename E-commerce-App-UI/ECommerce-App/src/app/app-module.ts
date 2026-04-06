import { NgModule, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { BrowserModule, provideClientHydration, withEventReplay } from '@angular/platform-browser';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { CoreModule } from './core/core-module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { Unauthorized } from './components/unauthorized/unauthorized';
import { TokenInterceptor } from './core/service/token.interceptor';
import { MainLayout } from './layout/main-layout/main-layout';
import { AuthLayout } from './layout/auth-layout/auth-layout';

import { AgGridModule } from 'ag-grid-angular';
import { MatDialogModule } from '@angular/material/dialog';

import { ToastrModule } from 'ngx-toastr';

@NgModule({
  declarations: [
    App,
    Login,
    Register,
    Unauthorized,
    MainLayout,
    AuthLayout,
    
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    BrowserAnimationsModule,
    CoreModule,  
    FormsModule,
    CommonModule,
    ReactiveFormsModule,
    AgGridModule,
    MatDialogModule,
     ToastrModule.forRoot({     // REQUIRED
      timeOut: 3000,
      closeButton: true,
      progressBar: true,
      positionClass: 'toast-top-right'
    })
    
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true },
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideClientHydration(withEventReplay())
  ],
  bootstrap: [App]
})
export class AppModule { }
