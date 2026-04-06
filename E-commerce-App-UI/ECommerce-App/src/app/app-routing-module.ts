import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { roleGuard } from './core/guards/role-guard';
import { roleRedirectGuard } from './core/guards/role-redirect-guard';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { AuthLayout } from './layout/auth-layout/auth-layout';
import { MainLayout } from './layout/main-layout/main-layout';

const routes: Routes = [
  // Public pages (Main layout - visible to everyone)
  {
    path: '',
    component: MainLayout,
    canActivate: [roleRedirectGuard],
    loadChildren: () =>
      import('./modules/user/user-module').then(m => m.UserModule),
  },

  // Auth pages (Login, Register — no header/footer)
  {
    path: '',
    component: AuthLayout,
    children: [
      { path: 'login', component: Login },
      { path: 'register', component: Register },
    ],
  },

  //User area (logged-in users only) — reuses same UserModule but under /user prefix
  {
    path: 'user',
    component: MainLayout,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['User'] },
    loadChildren: () =>
      import('./modules/user/user-module').then(m => m.UserModule),
  },

  // Admin area
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    loadChildren: () =>
      import('./modules/admin/admin-module').then(m => m.AdminModule),
  },

  // Supplier area (authenticated)
  {
    path: 'supplier',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Supplier'] },
    loadChildren: () =>
      import('./modules/supplier/supplier-module').then(m => m.SupplierModule),
  },

  // Supplier registration (public — no auth required)
  {
    path: 'supplier-register',
    component: AuthLayout,
    children: [
      {
        path: '',
        loadChildren: () =>
          import('./modules/supplier/supplier-module').then(m => m.SupplierModule),
      },
    ],
  },

  // 🚫 Fallback route
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    scrollPositionRestoration: 'top', // 👈 ensures every route starts at top
    anchorScrolling: 'enabled'
  })
  ],
  exports: [RouterModule],
})
export class AppRoutingModule {}
