import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../service/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  const allowedRoles: string[] = route.data?.['roles'] ?? [];
  const userRole = authService.getUserRole();

  if (allowedRoles.length === 0 || (userRole && allowedRoles.includes(userRole))) {
    return true;
  }

  router.navigate(['/unauthorized']);
  return false;
};
