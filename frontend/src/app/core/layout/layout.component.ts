import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, DestroyRef } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs/operators';
import { SidebarComponent, SidebarMenuItem } from './sidebar/sidebar.component';
import { adminSidebarMenu } from '../../features/admin/models/menu';
import { procurementSidebarMenu } from '../../features/procurement/models/menu';
import { supplierSidebarMenu } from '../../features/supplier/models/menu';
import { AuthService } from '../services/auth.service';
import { UserRole, UserSession } from '../../shared/models/user.model';
import { ThemeMode, ThemeService } from '../services/theme.service';

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
export class LayoutComponent implements AfterViewInit {
  title = 'qcharity-ui';

  activePageTitle = 'Dashboard';

  readonly sidebarMenus: Record<UserRole, SidebarMenuItem[]> = {
    Admin: adminSidebarMenu,
    Procurement: procurementSidebarMenu,
    Supplier: supplierSidebarMenu,
  } as const;

  readonly themeModes: { label: string; icon: string; value: ThemeMode }[] = [
    { label: 'Light', icon: 'ki-duotone ki-night-day', value: 'light' },
    { label: 'Dark', icon: 'ki-duotone ki-moon', value: 'dark' },
    { label: 'System', icon: 'ki-duotone ki-screen', value: 'system' },
  ];

  readonly themeMode$ = this.themeService.mode$;

  roles: UserRole[] = [];

  currentRole: UserRole = 'Admin';

  isAuthenticated = false;

  session: UserSession | null = null;

  get sidebarMenu() {
    return this.sidebarMenus[this.currentRole];
  }

  private layoutInitTimeout?: number;

  constructor(
    private readonly router: Router,
    destroyRef: DestroyRef,
    private readonly authService: AuthService,
    private readonly themeService: ThemeService,
  ) {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(destroyRef),
      )
      .subscribe(({ urlAfterRedirects }) => {
        this.updateActivePageTitle(urlAfterRedirects);
        this.scheduleLayoutInitialization();
      });

    this.authService.session$
      .pipe(takeUntilDestroyed(destroyRef))
      .subscribe((session) => this.handleSessionChange(session));
  }

  ngAfterViewInit() {
    this.scheduleLayoutInitialization();
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

  changeTheme(mode: ThemeMode) {
    this.themeService.setMode(mode);
    this.scheduleLayoutInitialization();
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
      this.activePageTitle = 'Dashboard';
      return;
    }

    this.roles = [session.role];
    this.currentRole = session.role;

    const defaultPath = this.authService.defaultPathForRole(session.role);
    const currentPath = this.router.url || window.location.pathname;
    const isLoginRoute = currentPath === '/' || currentPath.startsWith('/login');

    if (isLoginRoute) {
      this.router.navigateByUrl(defaultPath);
      return;
    }

    this.updateActivePageTitle(currentPath);

    this.scheduleLayoutInitialization();
  }

  private scheduleLayoutInitialization() {
    if (!this.isAuthenticated) {
      return;
    }

    if (this.layoutInitTimeout) {
      window.clearTimeout(this.layoutInitTimeout);
    }

    this.layoutInitTimeout = window.setTimeout(() => this.initializeLayoutScripts());
  }

  private initializeLayoutScripts() {
    const ktWindow = window as Window & {
      KTApp?: { init: () => void };
      KTMenu?: { init: () => void };
      KTComponents?: { init: () => void };
      KTAppSidebar?: { init: () => void };
    };

    ktWindow.KTComponents?.init?.();
    ktWindow.KTApp?.init?.();
    ktWindow.KTMenu?.init?.();
    ktWindow.KTAppSidebar?.init?.();
  }
}
