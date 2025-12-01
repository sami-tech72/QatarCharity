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

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'qcharity-ui';

  activePageTitle = 'Dashboard';

  readonly sidebarMenu = [
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
  ];

  constructor(private readonly router: Router, destroyRef: DestroyRef) {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(destroyRef),
      )
      .subscribe(({ urlAfterRedirects }) => {
        const match = this.sidebarMenu.find((item) => urlAfterRedirects.startsWith(item.path));
        this.activePageTitle = match?.title ?? this.sidebarMenu[0].title;
      });
  }
}
