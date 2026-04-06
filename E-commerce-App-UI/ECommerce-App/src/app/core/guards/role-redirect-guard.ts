import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../service/auth.service';

export const roleRedirectGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    const role = authService.getUserRole();
    if (role === 'Admin') {
      router.navigate(['/admin/dashboard']);
      return false;
    }
    if (role === 'Supplier') {
      router.navigate(['/supplier/dashboard']);
      return false;
    }
  }

  return true;
};
