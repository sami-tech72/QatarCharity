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
          import('./dashboard/procurement-dashboard.component').then((m) => m.ProcurementDashboardComponent),
      },
      {
        path: 'rfx-management',
        title: 'RFx Management',
        loadComponent: () =>
          import('./rfx-management/rfx-management.component').then((m) => m.RfxManagementComponent),
      },
      {
        path: 'rfx-management/create',
        title: 'Create RFx',
        loadComponent: () =>
          import('./rfx-management/create-rfx.component').then((m) => m.CreateRfxComponent),
      },
      {
        path: 'bid-evaluation',
        title: 'Bid Evaluation',
        loadComponent: () =>
          import('./bid-evaluation/bid-evaluation.component').then((m) => m.BidEvaluationComponent),
      },
      {
        path: 'tender-committee',
        title: 'Tender Committee',
        loadComponent: () =>
          import('./tender-committee/tender-committee.component').then((m) => m.TenderCommitteeComponent),
      },
      {
        path: 'contract-management',
        title: 'Contract Management',
        loadComponent: () =>
          import('./contract-management/contract-management.component').then((m) => m.ContractManagementComponent),
      },
      {
        path: 'supplier-performance',
        title: 'Supplier Performance',
        loadComponent: () =>
          import('./supplier-performance/supplier-performance.component').then((m) => m.SupplierPerformanceComponent),
      },
      {
        path: 'reports-analytics',
        title: 'Reports & Analytics',
        loadComponent: () =>
          import('./reports-analytics/reports-analytics.component').then((m) => m.ReportsAnalyticsComponent),
      },
      {
        path: 'roles-permissions',
        title: 'Roles & Permissions',
        loadComponent: () =>
          import('./roles-permissions/roles-permissions.component').then((m) => m.ProcurementRolesPermissionsComponent),
      },
      {
        path: 'settings',
        title: 'Procurement Settings',
        loadComponent: () => import('./settings/procurement-settings.component').then((m) => m.ProcurementSettingsComponent),
      },
    ],
  },
];
