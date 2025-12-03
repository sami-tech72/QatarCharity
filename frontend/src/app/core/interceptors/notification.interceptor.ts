import {
  HttpErrorResponse,
  HttpEvent,
  HttpInterceptorFn,
  HttpResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, tap } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { ApiResponse } from '../../shared/models/api-response.model';
import { NotificationService } from '../services/notification.service';

export const notificationInterceptor: HttpInterceptorFn = (req, next) => {
  const notifier = inject(NotificationService);

  return next(req).pipe(
    tap((event: HttpEvent<unknown>) => {
      if (!(event instanceof HttpResponse)) {
        return;
      }

      const body = event.body as ApiResponse<unknown> | undefined;

      if (!body?.message) {
        return;
      }

      if (body.success) {
        notifier.success(body.message);
      } else {
        notifier.error(body.message);
      }
    }),
    catchError((error: HttpErrorResponse) => {
      const message = error.error?.message ?? error.message ?? 'An unexpected error occurred.';
      notifier.error(message);
      return throwError(() => error);
    }),
  );
};
