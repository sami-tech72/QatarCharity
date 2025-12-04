import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { UserRole } from '../../shared/models/user.model';
import { AuthService } from '../services/auth.service';

export const authGuard: CanMatchFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const session = authService.currentSession();

  if (!session) {
    router.navigate(['/login']);
    return false;
  }

  const requiredRole = route.data?.['role'] as UserRole[] | UserRole | undefined;

  if (requiredRole) {
    const allowedRoles = Array.isArray(requiredRole) ? requiredRole : [requiredRole];

    if (!allowedRoles.includes(session.role)) {
      router.navigateByUrl(authService.defaultPathForRole(session.role));
      return false;
    }
  }

  return true;
};
