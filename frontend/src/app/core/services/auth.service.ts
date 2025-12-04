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
  ProcurementSubRole,
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
      tap((session) => this.persistSession(this.normalizeSession(session))),
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

  defaultPathForRole(role: UserRole, procurementSubRoles: ProcurementSubRole[] = []): string {
    const map: Record<UserRole, string> = {
      Admin: adminSidebarMenu[0].path,
      Procurement: this.firstProcurementPath(procurementSubRoles),
      Supplier: supplierSidebarMenu[0].path,
    };

    return map[role];
  }

  private firstProcurementPath(procurementSubRoles: ProcurementSubRole[]): string {
    if (!procurementSubRoles.length) {
      return procurementSidebarMenu[0].path;
    }

    const allowed = new Set<ProcurementSubRole>(procurementSubRoles);
    const match = procurementSidebarMenu.find((item) =>
      allowed.has(item.title as ProcurementSubRole),
    );

    return match?.path ?? procurementSidebarMenu[0].path;
  }

  private persistSession(session: UserSession) {
    const normalized = this.normalizeSession(session);
    localStorage.setItem(this.storageKey, JSON.stringify(normalized));
    this.sessionSubject.next(normalized);
  }

  private readSessionFromStorage(): UserSession | null {
    const raw = localStorage.getItem(this.storageKey);

    if (!raw) {
      return null;
    }

    try {
      const session = JSON.parse(raw) as UserSession;
      return this.normalizeSession(session);
    } catch (error) {
      console.error('Unable to parse stored session', error);
      return null;
    }
  }

  private normalizeSession(session: UserSession): UserSession {
    return {
      ...session,
      procurementSubRoles: session.procurementSubRoles ?? [],
    };
  }
}
