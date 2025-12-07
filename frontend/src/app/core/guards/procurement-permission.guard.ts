import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ProcurementPermissionAction } from '../../shared/models/procurement-roles.model';

export const procurementPermissionGuard: CanMatchFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const permissionName = route.data?.['permission'] as string | undefined;
  const permissionAction = (route.data?.['permissionAction'] as ProcurementPermissionAction | undefined) ?? 'read';

  if (!permissionName) {
    return true;
  }

  const session = authService.currentSession();

  if (!session) {
    router.navigate(['/login']);
    return false;
  }

  if (session.role === 'Admin') {
    return true;
  }

  if (session.role === 'Procurement' && !session.procurementRole) {
    return true;
  }

  if (!session.procurementRole) {
    router.navigate(['/login']);
    return false;
  }

  const permission = session.procurementRole.permissions.find((item) => item.name === permissionName);
  const allowed = permission?.actions?.[permissionAction];

  if (!allowed) {
    router.navigateByUrl(authService.defaultPathForRole(session.role));
    return false;
  }

  return true;
};
