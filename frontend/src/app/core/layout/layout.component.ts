import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, DestroyRef } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { SidebarComponent } from './sidebar/sidebar.component';
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

  readonly themeModes: { label: string; icon: string; value: ThemeMode }[] = [
    { label: 'Light', icon: 'ki-duotone ki-night-day', value: 'light' },
    { label: 'Dark', icon: 'ki-duotone ki-moon', value: 'dark' },
    { label: 'System', icon: 'ki-duotone ki-screen', value: 'system' },
  ];

  readonly themeMode$: Observable<ThemeMode>;

  roles: UserRole[] = [];

  currentRole: UserRole = 'Admin';

  isAuthenticated = false;

  session: UserSession | null = null;

  isCustomizerOpen = false;

  selectedPrimaryColor = this.getPrimaryColor();

  selectedSkin: 'default' | 'bordered' = 'default';

  isSemiDark = false;

  private readonly storageKeys = {
    primary: 'app-primary-color',
    skin: 'app-skin',
    semiDark: 'app-semi-dark',
  } as const;

  get sidebarMenu() {
    return this.authService.sidebarMenuForRole(this.currentRole, this.session);
  }

  private layoutInitTimeout?: number;

  constructor(
    private readonly router: Router,
    destroyRef: DestroyRef,
    private readonly authService: AuthService,
    private readonly themeService: ThemeService,
  ) {
    this.themeMode$ = this.themeService.mode$;

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

    this.restoreCustomizerState();
  }

  ngAfterViewInit() {
    this.scheduleLayoutInitialization();

    if (this.isSemiDark) {
      this.applySemiDarkToSidebar(true);
    }
  }

  logout() {
    this.authService.logout();
  }

  onRoleChange(role: UserRole) {
    if (this.currentRole === role) {
      return;
    }

    this.router.navigateByUrl(this.authService.defaultPathForRole(role, this.session));
  }

  changeTheme(mode: ThemeMode) {
    this.themeService.setMode(mode);
    this.scheduleLayoutInitialization();
  }

  toggleCustomizer(open?: boolean) {
    this.isCustomizerOpen = open ?? !this.isCustomizerOpen;
  }

  setPrimaryColor(color: string, persist = true) {
    this.selectedPrimaryColor = color;
    this.applyPrimaryColor(color);

    if (persist) {
      localStorage.setItem(this.storageKeys.primary, color);
    }
  }

  setSkin(skin: 'default' | 'bordered', persist = true) {
    this.selectedSkin = skin;
    document.body.classList.toggle('app-skin-bordered', skin === 'bordered');

    if (persist) {
      localStorage.setItem(this.storageKeys.skin, skin);
    }
  }

  toggleSemiDark(enabled: boolean, persist = true) {
    this.isSemiDark = enabled;
    document.body.classList.remove('app-semi-dark');
    this.applySemiDarkToSidebar(enabled);

    if (persist) {
      localStorage.setItem(this.storageKeys.semiDark, String(enabled));
    }
  }

  private applySemiDarkToSidebar(enabled: boolean) {
    const sidebar = document.getElementById('kt_app_sidebar');

    if (sidebar) {
      sidebar.classList.toggle('app-semi-dark', enabled);
    }
  }

  private updateActivePageTitle(url: string) {
    if (!this.isAuthenticated) {
      this.activePageTitle = 'Dashboard';
      return;
    }

    const sidebarMenu = this.sidebarMenu;

    if (sidebarMenu.length === 0) {
      this.activePageTitle = 'Dashboard';
      return;
    }

    const match = sidebarMenu.find((item) => url.startsWith(item.path));
    this.activePageTitle = match?.title ?? sidebarMenu[0].title;
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

    const browserPath = window.location.pathname;
    const currentPath =
      this.router.url && this.router.url !== '/' ? this.router.url : browserPath;
    const isLoginRoute = currentPath === '/' || currentPath.startsWith('/login');

    if (isLoginRoute) {
      const defaultPath = this.authService.defaultPathForRole(session.role, session);
      this.router.navigateByUrl(defaultPath);
      return;
    }

    this.updateActivePageTitle(currentPath);

    this.scheduleLayoutInitialization();
  }

  private restoreCustomizerState() {
    const storedPrimary = localStorage.getItem(this.storageKeys.primary);
    const storedSkin = localStorage.getItem(this.storageKeys.skin) as
      | 'default'
      | 'bordered'
      | null;
    const storedSemiDark = localStorage.getItem(this.storageKeys.semiDark);

    const primaryToApply = storedPrimary || this.selectedPrimaryColor;
    this.setPrimaryColor(primaryToApply, false);

    if (storedSkin === 'default' || storedSkin === 'bordered') {
      this.setSkin(storedSkin, false);
    }

    if (storedSemiDark !== null) {
      this.toggleSemiDark(storedSemiDark === 'true', false);
    }
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

  getPrimaryColor() {
    const computedStyle = getComputedStyle(document.documentElement);
    return computedStyle.getPropertyValue('--app-primary').trim() || '#0d6efd';
  }

  private applyPrimaryColor(color: string) {
    const rgb = this.hexToRgb(color);

    document.documentElement.style.setProperty('--app-primary', color);
    document.documentElement.style.setProperty('--bs-primary', color);
    document.body.style.setProperty('--app-primary', color);
    document.body.style.setProperty('--bs-primary', color);

    if (rgb) {
      document.documentElement.style.setProperty('--bs-primary-rgb', rgb.join(','));
      document.body.style.setProperty('--bs-primary-rgb', rgb.join(','));
    }
  }

  private hexToRgb(hex: string): [number, number, number] | null {
    const cleaned = hex.replace('#', '');

    if (cleaned.length !== 6) {
      return null;
    }

    const bigint = parseInt(cleaned, 16);

    return [
      (bigint >> 16) & 255,
      (bigint >> 8) & 255,
      bigint & 255,
    ];
  }
}
