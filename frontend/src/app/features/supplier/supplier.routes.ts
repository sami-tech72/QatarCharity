import { Routes } from '@angular/router';

export const supplierRoutes: Routes = [
  {
    path: 'supplier',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Supplier Dashboard',
        loadComponent: () =>
          import('./pages/dashboard/supplier-dashboard.component').then((m) => m.SupplierDashboardComponent),
      },
      {
        path: 'available-tenders',
        title: 'Available Tenders',
        loadComponent: () =>
          import('./pages/available-tenders/available-tenders.component').then((m) => m.AvailableTendersComponent),
      },
      {
        path: 'my-bids',
        title: 'My Bids',
        loadComponent: () => import('./pages/my-bids/my-bids.component').then((m) => m.MyBidsComponent),
      },
      {
        path: 'my-contracts',
        title: 'My Contracts',
        loadComponent: () =>
          import('./pages/my-contracts/my-contracts.component').then((m) => m.MyContractsComponent),
      },
      {
        path: 'performance',
        title: 'Performance',
        loadComponent: () => import('./pages/performance/performance.component').then((m) => m.PerformanceComponent),
      },
      {
        path: 'company-profile',
        title: 'Company Profile',
        loadComponent: () =>
          import('./pages/company-profile/company-profile.component').then((m) => m.CompanyProfileComponent),
      },
      {
        path: 'documents',
        title: 'Documents',
        loadComponent: () => import('./pages/documents/documents.component').then((m) => m.DocumentsComponent),
      },
      {
        path: 'settings',
        title: 'Supplier Settings',
        loadComponent: () => import('./pages/settings/supplier-settings.component').then((m) => m.SupplierSettingsComponent),
      },
    ],
  },
];
