import { Routes } from '@angular/router';

export const procurementRoutes: Routes = [
  {
    path: 'procurement',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Procurement Dashboard',
        data: { permission: 'dashboard' },
        loadComponent: () =>
          import('./dashboard/procurement-dashboard.component').then((m) => m.ProcurementDashboardComponent),
      },
      {
        path: 'rfx-management',
        title: 'RFx Management',
        data: { permission: 'rfx-management' },
        loadComponent: () =>
          import('./rfx-management/rfx-management.component').then((m) => m.RfxManagementComponent),
      },
      {
        path: 'rfx-management/create',
        title: 'Create RFx',
        data: { permission: 'rfx-management:create' },
        loadComponent: () =>
          import('./rfx-management/create-rfx.component').then((m) => m.CreateRfxComponent),
      },
      {
        path: 'bid-evaluation',
        title: 'Bid Evaluation',
        data: { permission: 'bid-evaluation' },
        loadComponent: () =>
          import('./bid-evaluation/bid-evaluation.component').then((m) => m.BidEvaluationComponent),
      },
      {
        path: 'tender-committee',
        title: 'Tender Committee',
        data: { permission: 'tender-committee' },
        loadComponent: () =>
          import('./tender-committee/tender-committee.component').then((m) => m.TenderCommitteeComponent),
      },
      {
        path: 'contract-management',
        title: 'Contract Management',
        data: { permission: 'contract-management' },
        loadComponent: () =>
          import('./contract-management/contract-management.component').then((m) => m.ContractManagementComponent),
      },
      {
        path: 'supplier-performance',
        title: 'Supplier Performance',
        data: { permission: 'supplier-performance' },
        loadComponent: () =>
          import('./supplier-performance/supplier-performance.component').then((m) => m.SupplierPerformanceComponent),
      },
      {
        path: 'reports-analytics',
        title: 'Reports & Analytics',
        data: { permission: 'reports-analytics' },
        loadComponent: () =>
          import('./reports-analytics/reports-analytics.component').then((m) => m.ReportsAnalyticsComponent),
      },
      {
        path: 'settings',
        title: 'Procurement Settings',
        data: { permission: 'settings' },
        loadComponent: () => import('./settings/procurement-settings.component').then((m) => m.ProcurementSettingsComponent),
      },
    ],
  },
];
