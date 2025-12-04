import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { ProcurementPermissionSet, UserRole } from '../../shared/models/user.model';
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
  const requiredProcurementPermission = route.data?.['procurementPermission'] as
    | keyof ProcurementPermissionSet
    | undefined;

  if (requiredRole && session.role !== requiredRole) {
    router.navigateByUrl(authService.defaultPathForRole(session.role));
    return false;
  }

  if (requiredProcurementPermission && !authService.hasProcurementPermission(requiredProcurementPermission)) {
    router.navigateByUrl(authService.defaultPathForRole(session.role));
    return false;
  }

  return true;
};
