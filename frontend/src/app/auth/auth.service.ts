import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { adminSidebarMenu } from '../roles/admin/menu';
import { procurementSidebarMenu } from '../roles/procurement/menu';
import { supplierSidebarMenu } from '../roles/supplier/menu';
import { SidebarRole } from '../components/sidebar/sidebar.component';
import { LoginRequest, LoginResponse, UserSession } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiBaseUrl = 'http://localhost:5112/api';
  private readonly storageKey = 'qcharity-session';

  private readonly sessionSubject = new BehaviorSubject<UserSession | null>(
    this.readSessionFromStorage(),
  );

  readonly session$: Observable<UserSession | null> = this.sessionSubject.asObservable();

  constructor(private readonly http: HttpClient, private readonly router: Router) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiBaseUrl}/auth/login`, request).pipe(
      tap((response) => this.persistSession(response)),
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

  defaultPathForRole(role: SidebarRole): string {
    const map: Record<SidebarRole, string> = {
      Admin: adminSidebarMenu[0].path,
      Procurement: procurementSidebarMenu[0].path,
      Supplier: supplierSidebarMenu[0].path,
    };

    return map[role];
  }

  private persistSession(response: LoginResponse) {
    const session: UserSession = {
      ...response,
    };

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
