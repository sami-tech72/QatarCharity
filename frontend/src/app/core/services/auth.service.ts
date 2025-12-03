import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, catchError, map, of, tap, throwError } from 'rxjs';
import { adminSidebarMenu } from '../../features/admin/models/menu';
import { procurementSidebarMenu } from '../../features/procurement/models/menu';
import { supplierSidebarMenu } from '../../features/supplier/models/menu';
import { UserRole, LoginRequest, LoginResponse, UserSession } from '../../shared/models/user.model';
import { ApiService } from './api.service';
import { ApiResponse } from '../../shared/models/api-response.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'qcharity-session';

  private readonly demoAccounts: Record<string, { password: string; role: UserRole; displayName: string }> = {
    'admin@qcharity.test': {
      password: 'P@ssw0rd!',
      role: 'Admin',
      displayName: 'Admin User',
    },
    'procurement@qcharity.test': {
      password: 'P@ssw0rd!',
      role: 'Procurement',
      displayName: 'Procurement User',
    },
    'supplier@qcharity.test': {
      password: 'P@ssw0rd!',
      role: 'Supplier',
      displayName: 'Supplier User',
    },
  };

  private readonly sessionSubject = new BehaviorSubject<UserSession | null>(
    this.readSessionFromStorage(),
  );

  readonly session$: Observable<UserSession | null> = this.sessionSubject.asObservable();

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
  ) {}

  login(request: LoginRequest): Observable<UserSession> {
    const cachedDemoSession = this.validateDemoCredentials(request);

    if (cachedDemoSession) {
      return of(cachedDemoSession).pipe(tap((session) => this.persistSession(session)));
    }

    return this.api.post<LoginResponse>('auth/login', request).pipe(
      map((response: ApiResponse<LoginResponse>) => {
        if (!response.success || !response.data) {
          throw new Error(response.message || 'Login failed.');
        }

        return response.data;
      }),
      tap((session) => this.persistSession(session)),
      catchError((error) => {
        const fallbackSession = this.validateDemoCredentials(request);

        if (fallbackSession) {
          return of(fallbackSession).pipe(tap((session) => this.persistSession(session)));
        }

        return throwError(() => error);
      }),
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

  private validateDemoCredentials(request: LoginRequest): UserSession | null {
    const email = request.email.trim().toLowerCase();
    const account = this.demoAccounts[email];

    if (!account || request.password !== account.password) {
      return null;
    }

    return {
      email,
      displayName: account.displayName,
      role: account.role,
      token: `demo-token-${account.role.toLowerCase()}`,
      expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    };
  }
}
