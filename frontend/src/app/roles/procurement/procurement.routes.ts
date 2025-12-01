import { Routes } from '@angular/router';

export const procurementRoutes: Routes = [
  {
    path: 'procurement',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Procurement Dashboard',
        loadComponent: () =>
          import('./pages/dashboard/procurement-dashboard.component').then((m) => m.ProcurementDashboardComponent),
      },
      {
        path: 'rfx-management',
        title: 'RFx Management',
        loadComponent: () =>
          import('./pages/rfx-management/rfx-management.component').then((m) => m.RfxManagementComponent),
      },
      {
        path: 'bid-evaluation',
        title: 'Bid Evaluation',
        loadComponent: () =>
          import('./pages/bid-evaluation/bid-evaluation.component').then((m) => m.BidEvaluationComponent),
      },
      {
        path: 'tender-committee',
        title: 'Tender Committee',
        loadComponent: () =>
          import('./pages/tender-committee/tender-committee.component').then((m) => m.TenderCommitteeComponent),
      },
      {
        path: 'contract-management',
        title: 'Contract Management',
        loadComponent: () =>
          import('./pages/contract-management/contract-management.component').then((m) => m.ContractManagementComponent),
      },
      {
        path: 'supplier-performance',
        title: 'Supplier Performance',
        loadComponent: () =>
          import('./pages/supplier-performance/supplier-performance.component').then((m) => m.SupplierPerformanceComponent),
      },
      {
        path: 'reports-analytics',
        title: 'Reports & Analytics',
        loadComponent: () =>
          import('./pages/reports-analytics/reports-analytics.component').then((m) => m.ReportsAnalyticsComponent),
      },
      {
        path: 'settings',
        title: 'Procurement Settings',
        loadComponent: () => import('./pages/settings/procurement-settings.component').then((m) => m.ProcurementSettingsComponent),
      },
    ],
  },
];
