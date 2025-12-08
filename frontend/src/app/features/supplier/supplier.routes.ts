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
          import('./dashboard/supplier-dashboard.component').then((m) => m.SupplierDashboardComponent),
      },
      {
        path: 'available-tenders',
        title: 'Available Tenders',
        loadComponent: () =>
          import('./available-tenders/available-tenders.component').then((m) => m.AvailableTendersComponent),
      },
      {
        path: 'available-tenders/:id/bid',
        title: 'Submit Bid',
        loadComponent: () =>
          import('./available-tenders/tender-bid/tender-bid.component').then((m) => m.TenderBidComponent),
      },
      {
        path: 'my-bids',
        title: 'My Bids',
        loadComponent: () => import('./my-bids/my-bids.component').then((m) => m.MyBidsComponent),
      },
      {
        path: 'my-contracts',
        title: 'My Contracts',
        loadComponent: () =>
          import('./my-contracts/my-contracts.component').then((m) => m.MyContractsComponent),
      },
      {
        path: 'performance',
        title: 'Performance',
        loadComponent: () => import('./performance/performance.component').then((m) => m.PerformanceComponent),
      },
      {
        path: 'company-profile',
        title: 'Company Profile',
        loadComponent: () =>
          import('./company-profile/company-profile.component').then((m) => m.CompanyProfileComponent),
      },
      {
        path: 'documents',
        title: 'Documents',
        loadComponent: () => import('./documents/documents.component').then((m) => m.DocumentsComponent),
      },
      {
        path: 'settings',
        title: 'Supplier Settings',
        loadComponent: () => import('./settings/supplier-settings.component').then((m) => m.SupplierSettingsComponent),
      },
    ],
  },
];
