import { Routes } from '@angular/router';
import {
  PROCUREMENT_ALL_SUB_ROLES,
  PROCUREMENT_MANAGERS_AND_OFFICERS,
  PROCUREMENT_MANAGERS_ONLY,
} from './models/permissions';

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
        data: { allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS },
      },
      {
        path: 'rfx-management/create',
        title: 'Create RFx',
        loadComponent: () =>
          import('./rfx-management/create-rfx.component').then((m) => m.CreateRfxComponent),
        data: { allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS },
      },
      {
        path: 'bid-evaluation',
        title: 'Bid Evaluation',
        loadComponent: () =>
          import('./bid-evaluation/bid-evaluation.component').then((m) => m.BidEvaluationComponent),
        data: { allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS },
      },
      {
        path: 'tender-committee',
        title: 'Tender Committee',
        loadComponent: () =>
          import('./tender-committee/tender-committee.component').then((m) => m.TenderCommitteeComponent),
        data: { allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS },
      },
      {
        path: 'contract-management',
        title: 'Contract Management',
        loadComponent: () =>
          import('./contract-management/contract-management.component').then((m) => m.ContractManagementComponent),
        data: { allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS },
      },
      {
        path: 'supplier-performance',
        title: 'Supplier Performance',
        loadComponent: () =>
          import('./supplier-performance/supplier-performance.component').then((m) => m.SupplierPerformanceComponent),
        data: { allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS },
      },
      {
        path: 'reports-analytics',
        title: 'Reports & Analytics',
        loadComponent: () =>
          import('./reports-analytics/reports-analytics.component').then((m) => m.ReportsAnalyticsComponent),
        data: { allowedSubRoles: PROCUREMENT_ALL_SUB_ROLES },
      },
      {
        path: 'settings',
        title: 'Procurement Settings',
        loadComponent: () => import('./settings/procurement-settings.component').then((m) => m.ProcurementSettingsComponent),
        data: { allowedSubRoles: PROCUREMENT_MANAGERS_ONLY },
      },
    ],
  },
];
