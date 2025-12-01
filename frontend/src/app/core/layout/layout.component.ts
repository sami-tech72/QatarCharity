import { CommonModule } from '@angular/common';
import { Component, DestroyRef } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs/operators';
import { SidebarComponent, SidebarMenuItem } from './sidebar/sidebar.component';
import { adminSidebarMenu } from '../../features/admin/models/menu';
import { procurementSidebarMenu } from '../../features/procurement/models/menu';
import { supplierSidebarMenu } from '../../features/supplier/models/menu';
import { AuthService } from '../services/auth.service';
import { UserRole, UserSession } from '../../shared/models/user.model';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    SidebarComponent,
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  title = 'qcharity-ui';

  activePageTitle = 'Dashboard';

  readonly sidebarMenus: Record<UserRole, SidebarMenuItem[]> = {
    Admin: adminSidebarMenu,
    Procurement: procurementSidebarMenu,
    Supplier: supplierSidebarMenu,
  } as const;

  roles: UserRole[] = [];

  currentRole: UserRole = 'Admin';

  isAuthenticated = false;

  session: UserSession | null = null;

  get sidebarMenu() {
    return this.sidebarMenus[this.currentRole];
  }

  constructor(
    private readonly router: Router,
    destroyRef: DestroyRef,
    private readonly authService: AuthService,
  ) {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(destroyRef),
      )
      .subscribe(({ urlAfterRedirects }) => this.updateActivePageTitle(urlAfterRedirects));

    this.authService.session$
      .pipe(takeUntilDestroyed(destroyRef))
      .subscribe((session) => this.handleSessionChange(session));
  }

  logout() {
    this.authService.logout();
  }

  onRoleChange(role: UserRole) {
    if (this.currentRole === role) {
      return;
    }

    this.router.navigateByUrl(this.authService.defaultPathForRole(role));
  }

  private updateActivePageTitle(url: string) {
    if (!this.isAuthenticated) {
      this.activePageTitle = 'Dashboard';
      return;
    }

    const match = this.sidebarMenu.find((item) => url.startsWith(item.path));
    this.activePageTitle = match?.title ?? this.sidebarMenu[0].title;
  }

  private handleSessionChange(session: UserSession | null) {
    this.session = session;
    this.isAuthenticated = !!session;

    if (!session) {
      this.roles = [];
      return;
    }

    this.roles = [session.role];
    this.currentRole = session.role;
    this.activePageTitle = this.sidebarMenu[0]?.title ?? 'Dashboard';

    const defaultPath = this.authService.defaultPathForRole(session.role);

    if (!this.router.url.startsWith(defaultPath)) {
      this.router.navigateByUrl(defaultPath);
    }
  }
}
