import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { ProcurementSubRole, UserRole } from '../../shared/models/user.model';
import { AuthService } from '../services/auth.service';

export const authGuard: CanMatchFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const session = authService.currentSession();

  if (!session) {
    router.navigate(['/login']);
    return false;
  }

  const requiredRole = route.data?.['role'] as UserRole | undefined;
  const allowedSubRoles = route.data?.['allowedSubRoles'] as ProcurementSubRole[] | undefined;

  if (requiredRole && session.role !== requiredRole) {
    router.navigateByUrl(authService.defaultPathForRole(session.role));
    return false;
  }

  if (
    requiredRole === 'Procurement' &&
    allowedSubRoles?.length &&
    !allowedSubRoles.some((subRole) => session.subRoles.includes(subRole))
  ) {
    router.navigateByUrl(authService.defaultPathForRole(session.role));
    return false;
  }

  return true;
};
