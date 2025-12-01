import { CommonModule } from '@angular/common';
import { Component, DestroyRef } from '@angular/core';
import {
  NavigationEnd,
  Router,
  RouterLink,
  RouterOutlet,
} from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs/operators';
import {
  SidebarComponent,
  SidebarMenuItem,
  SidebarRole,
} from './components/sidebar/sidebar.component';
import { adminSidebarMenu } from './roles/admin/menu';
import { procurementSidebarMenu } from './roles/procurement/menu';
import { supplierSidebarMenu } from './roles/supplier/menu';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    SidebarComponent,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'qcharity-ui';

  activePageTitle = 'Dashboard';

  readonly sidebarMenus: Record<SidebarRole, SidebarMenuItem[]> = {
    Admin: adminSidebarMenu,
    Procurement: procurementSidebarMenu,
    Supplier: supplierSidebarMenu,
  } as const;

  readonly roles: SidebarRole[] = Object.keys(this.sidebarMenus) as SidebarRole[];

  currentRole: SidebarRole = 'Admin';
  isLoginRoute = false;

  get sidebarMenu() {
    return this.sidebarMenus[this.currentRole];
  }

  constructor(private readonly router: Router, destroyRef: DestroyRef) {
    this.updateActivePageTitle(this.router.url);
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(destroyRef),
      )
      .subscribe(({ urlAfterRedirects }) => this.updateActivePageTitle(urlAfterRedirects));
  }

  setRole(role: SidebarRole) {
    if (this.currentRole === role) {
      return;
    }

    this.currentRole = role;
    const activeUrl = this.router.url;
    const match = this.sidebarMenu.find((item) => activeUrl.startsWith(item.path));

    if (!match) {
      const [firstItem] = this.sidebarMenu;
      this.activePageTitle = firstItem.title;
      this.router.navigateByUrl(firstItem.path);
      return;
    }

    this.activePageTitle = match.title;
  }

  private updateActivePageTitle(url: string) {
    const normalizedUrl = url.startsWith('/') ? url : `/${url}`;
    this.isLoginRoute = normalizedUrl.startsWith('/login');

    if (this.isLoginRoute) {
      this.activePageTitle = 'Login';
      return;
    }

    const match = this.sidebarMenu.find((item) => normalizedUrl.startsWith(item.path));
    this.activePageTitle = match?.title ?? this.sidebarMenu[0].title;
  }
}
