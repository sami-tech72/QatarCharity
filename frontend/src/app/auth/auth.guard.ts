import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { SidebarRole } from '../components/sidebar/sidebar.component';
import { AuthService } from './auth.service';

export const authGuard: CanMatchFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const session = authService.currentSession();

  if (!session) {
    router.navigate(['/login']);
    return false;
  }

  const requiredRole = route.data?.['role'] as SidebarRole | undefined;

  if (requiredRole && session.role !== requiredRole) {
    router.navigateByUrl(authService.defaultPathForRole(session.role));
    return false;
  }

  return true;
};
