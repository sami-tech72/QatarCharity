import { CommonModule } from '@angular/common';
import { Component, DestroyRef } from '@angular/core';
import {
  NavigationEnd,
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
} from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs/operators';
import {
  SidebarComponent,
  SidebarMenuItem,
  SidebarRole,
} from './components/sidebar/sidebar.component';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    SidebarComponent,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'qcharity-ui';

  activePageTitle = 'Dashboard';

  readonly sidebarMenus: Record<SidebarRole, SidebarMenuItem[]> = {
    Admin: [
      { title: 'Dashboard', icon: 'ki-duotone ki-element-11 fs-2', path: '/dashboard' },
      { title: 'User Management', icon: 'ki-duotone ki-people fs-2', path: '/user-management' },
      {
        title: 'Supplier Management',
        icon: 'ki-duotone ki-briefcase fs-2',
        path: '/supplier-management',
      },
      {
        title: 'Workflow Configuration',
        icon: 'ki-duotone ki-setting-3 fs-2',
        path: '/workflow-configuration',
      },
      {
        title: 'Document Templates',
        icon: 'ki-duotone ki-folder fs-2',
        path: '/document-templates',
      },
      {
        title: 'System Integrations',
        icon: 'ki-duotone ki-abstract-33 fs-2',
        path: '/system-integrations',
      },
      { title: 'Audit Logs', icon: 'ki-duotone ki-shield-search fs-2', path: '/audit-logs' },
      {
        title: 'System Settings',
        icon: 'ki-duotone ki-setting-2 fs-2',
        path: '/system-settings',
      },
    ],
    Procurement: [
      { title: 'Dashboard', icon: 'ki-duotone ki-element-11 fs-2', path: '/dashboard' },
      { title: 'RFx Management', icon: 'ki-duotone ki-row-horizontal fs-2', path: '/rfx-management' },
      { title: 'Bid Evaluation', icon: 'ki-duotone ki-abstract-44 fs-2', path: '/bid-evaluation' },
      { title: 'Tender Committee', icon: 'ki-duotone ki-people fs-2', path: '/tender-committee' },
      { title: 'Contract Management', icon: 'ki-duotone ki-file-added fs-2', path: '/contract-management' },
      { title: 'Supplier Performance', icon: 'ki-duotone ki-activity fs-2', path: '/supplier-performance' },
      { title: 'Reports & Analytics', icon: 'ki-duotone ki-chart-line-up fs-2', path: '/reports-analytics' },
      { title: 'Settings', icon: 'ki-duotone ki-setting-2 fs-2', path: '/settings' },
    ],
    Supplier: [
      { title: 'Dashboard', icon: 'ki-duotone ki-element-11 fs-2', path: '/dashboard' },
      { title: 'Available Tenders', icon: 'ki-duotone ki-search-list fs-2', path: '/available-tenders' },
      { title: 'My Bids', icon: 'ki-duotone ki-send fs-2', path: '/my-bids' },
      { title: 'My Contracts', icon: 'ki-duotone ki-folder-up fs-2', path: '/my-contracts' },
      { title: 'Performance', icon: 'ki-duotone ki-chart-line-up fs-2', path: '/performance' },
      { title: 'Company Profile', icon: 'ki-duotone ki-profile-user fs-2', path: '/company-profile' },
      { title: 'Documents', icon: 'ki-duotone ki-file-up fs-2', path: '/documents' },
      { title: 'Settings', icon: 'ki-duotone ki-setting-2 fs-2', path: '/settings' },
    ],
  } as const;

  readonly roles: SidebarRole[] = Object.keys(this.sidebarMenus) as SidebarRole[];

  currentRole: SidebarRole = 'Admin';

  get sidebarMenu() {
    return this.sidebarMenus[this.currentRole];
  }

  constructor(private readonly router: Router, destroyRef: DestroyRef) {
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
    const match = this.sidebarMenu.find((item) => url.startsWith(item.path));
    this.activePageTitle = match?.title ?? this.sidebarMenu[0].title;
  }
}
