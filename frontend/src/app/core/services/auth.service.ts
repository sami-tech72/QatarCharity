import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import {
  BehaviorSubject,
  Observable,
  catchError,
  firstValueFrom,
  map,
  tap,
  throwError,
} from 'rxjs';
import { adminSidebarMenu } from '../../features/admin/models/menu';
import { procurementSidebarMenu } from '../../features/procurement/models/menu';
import { supplierSidebarMenu } from '../../features/supplier/models/menu';
import { ProcurementPermissionAction } from '../../shared/models/procurement-roles.model';
import { UserRole, LoginRequest, LoginResponse, UserSession } from '../../shared/models/user.model';
import { SidebarMenuItem } from '../layout/sidebar/sidebar.component';
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

  async initialize(): Promise<void> {
    const session = this.currentSession();

    if (!session) {
      return;
    }

    try {
      const freshSession = await firstValueFrom(
        this.api.get<LoginResponse>('auth/me').pipe(
          map((response: ApiResponse<LoginResponse>) => {
            if (!response.success || !response.data) {
              throw new Error(response.message || 'Unable to verify session.');
            }

            return response.data;
          }),
        ),
      );

      this.persistSession(freshSession);
    } catch {
      this.logout();
    }
  }

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

  logout(shouldRedirect = true) {
    localStorage.removeItem(this.storageKey);
    this.sessionSubject.next(null);

    if (shouldRedirect) {
      this.router.navigate(['/login']);
    }
  }

  currentSession(): UserSession | null {
    return this.sessionSubject.value;
  }

  sidebarMenuForRole(role: UserRole, session?: UserSession | null): SidebarMenuItem[] {
    const menuMap: Record<UserRole, SidebarMenuItem[]> = {
      Admin: adminSidebarMenu,
      Procurement: procurementSidebarMenu,
      Supplier: supplierSidebarMenu,
    };

    const baseMenu = menuMap[role] ?? [];

    if (role !== 'Procurement') {
      return baseMenu;
    }

    return this.filterProcurementMenu(baseMenu, session ?? this.currentSession());
  }

  defaultPathForRole(role: UserRole, session?: UserSession | null): string {
    const menu = this.sidebarMenuForRole(role, session ?? this.currentSession());

    return menu[0]?.path ?? '/';
  }

  private persistSession(session: UserSession) {
    localStorage.setItem(this.storageKey, JSON.stringify(session));
    this.sessionSubject.next(session);
  }

  private filterProcurementMenu(menu: SidebarMenuItem[], session: UserSession | null): SidebarMenuItem[] {
    if (!session?.procurementRole) {
      return menu;
    }

    return menu.filter((item) => this.hasProcurementPermission(session, item));
  }

  private hasProcurementPermission(session: UserSession | null, item: SidebarMenuItem): boolean {
    const requirement = item.permission;

    if (!requirement) {
      return true;
    }

    const procurementRole = session?.procurementRole;

    if (!procurementRole) {
      return false;
    }

    const action: ProcurementPermissionAction = requirement.action ?? 'read';
    const permission = procurementRole.permissions.find((perm) => perm.name === requirement.name);

    return !!permission?.actions?.[action];
  }

  private readSessionFromStorage(): UserSession | null {
    const raw = localStorage.getItem(this.storageKey);

    if (!raw) {
      return null;
    }

    try {
      const session = JSON.parse(raw) as UserSession;

      if (this.isExpired(session.expiresAt)) {
        localStorage.removeItem(this.storageKey);
        return null;
      }

      return session;
    } catch (error) {
      console.error('Unable to parse stored session', error);
      return null;
    }
  }

  private isExpired(expiresAt: string | Date): boolean {
    return new Date(expiresAt).getTime() <= Date.now();
  }
}
