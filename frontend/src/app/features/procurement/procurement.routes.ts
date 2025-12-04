import { Routes } from '@angular/router';
import { ProcurementPermission } from '../../shared/models/user.model';

export const procurementRoutes: Routes = [
  {
    path: 'procurement',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Procurement Dashboard',
        data: { permission: 'dashboard' as ProcurementPermission },
        loadComponent: () =>
          import('./dashboard/procurement-dashboard.component').then((m) => m.ProcurementDashboardComponent),
      },
      {
        path: 'rfx-management',
        title: 'RFx Management',
        data: { permission: 'rfx-management' as ProcurementPermission },
        loadComponent: () =>
          import('./rfx-management/rfx-management.component').then((m) => m.RfxManagementComponent),
      },
      {
        path: 'rfx-management/create',
        title: 'Create RFx',
        data: { permission: 'rfx-management:create' as ProcurementPermission },
        loadComponent: () =>
          import('./rfx-management/create-rfx.component').then((m) => m.CreateRfxComponent),
      },
      {
        path: 'bid-evaluation',
        title: 'Bid Evaluation',
        data: { permission: 'bid-evaluation' as ProcurementPermission },
        loadComponent: () =>
          import('./bid-evaluation/bid-evaluation.component').then((m) => m.BidEvaluationComponent),
      },
      {
        path: 'tender-committee',
        title: 'Tender Committee',
        data: { permission: 'tender-committee' as ProcurementPermission },
        loadComponent: () =>
          import('./tender-committee/tender-committee.component').then((m) => m.TenderCommitteeComponent),
      },
      {
        path: 'contract-management',
        title: 'Contract Management',
        data: { permission: 'contract-management' as ProcurementPermission },
        loadComponent: () =>
          import('./contract-management/contract-management.component').then((m) => m.ContractManagementComponent),
      },
      {
        path: 'supplier-performance',
        title: 'Supplier Performance',
        data: { permission: 'supplier-performance' as ProcurementPermission },
        loadComponent: () =>
          import('./supplier-performance/supplier-performance.component').then((m) => m.SupplierPerformanceComponent),
      },
      {
        path: 'reports-analytics',
        title: 'Reports & Analytics',
        data: { permission: 'reports-analytics' as ProcurementPermission },
        loadComponent: () =>
          import('./reports-analytics/reports-analytics.component').then((m) => m.ReportsAnalyticsComponent),
      },
      {
        path: 'settings',
        title: 'Procurement Settings',
        data: { permission: 'settings' as ProcurementPermission },
        loadComponent: () => import('./settings/procurement-settings.component').then((m) => m.ProcurementSettingsComponent),
      },
    ],
  },
];
