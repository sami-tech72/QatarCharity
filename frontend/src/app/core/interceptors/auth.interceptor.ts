import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const session = inject(AuthService).currentSession();

  if (!session || req.url.startsWith('/assets')) {
    return next(req);
  }

  const cloned = req.clone({
    setHeaders: {
      Authorization: `Bearer ${session.token}`,
    },
  });

  return next(cloned);
};
