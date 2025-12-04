import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, catchError, map, tap, throwError } from 'rxjs';
import { adminSidebarMenu } from '../../features/admin/models/menu';
import { procurementSidebarMenu } from '../../features/procurement/models/menu';
import { supplierSidebarMenu } from '../../features/supplier/models/menu';
import {
  UserRole,
  LoginRequest,
  LoginResponse,
  UserSession,
  ProcurementPermission,
} from '../../shared/models/user.model';
import { ApiService } from './api.service';
import { ApiResponse } from '../../shared/models/api-response.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'qcharity-session';

  private readonly sessionSubject = new BehaviorSubject<UserSession | null>(
    this.readSessionFromStorage(),
  );

  readonly session$: Observable<UserSession | null> = this.sessionSubject.asObservable();

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
  ) {}

  login(request: LoginRequest): Observable<UserSession> {
    return this.api.post<LoginResponse>('auth/login', request).pipe(
      map((response: ApiResponse<LoginResponse>) => {
        if (!response.success || !response.data) {
          throw new Error(response.message || 'Login failed.');
        }

        return response.data;
      }),
      tap((session) => this.persistSession(session)),
      catchError((error) => throwError(() => error)),
    );
  }

  logout() {
    localStorage.removeItem(this.storageKey);
    this.sessionSubject.next(null);
    this.router.navigate(['/login']);
  }

  currentSession(): UserSession | null {
    return this.sessionSubject.value;
  }

  defaultPathForRole(role: UserRole): string {
    const map: Record<UserRole, string> = {
      Admin: adminSidebarMenu[0].path,
      Procurement: procurementSidebarMenu[0].path,
      Supplier: supplierSidebarMenu[0].path,
    };

    return map[role];
  }

  hasProcurementPermission(permission: ProcurementPermission): boolean {
    const session = this.currentSession();

    if (!session || session.role !== 'Procurement') {
      return false;
    }

    return session.procurementAccess?.permissions?.includes(permission) ?? false;
  }

  private persistSession(session: UserSession) {
    localStorage.setItem(this.storageKey, JSON.stringify(session));
    this.sessionSubject.next(session);
  }

  private readSessionFromStorage(): UserSession | null {
    const raw = localStorage.getItem(this.storageKey);

    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as UserSession;
    } catch (error) {
      console.error('Unable to parse stored session', error);
      return null;
    }
  }
}
