import { Routes } from '@angular/router';
import { procurementPermissionGuard } from '../../core/guards/procurement-permission.guard';

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
        canMatch: [procurementPermissionGuard],
        data: { permission: 'RFx Management', permissionAction: 'read' },
        loadComponent: () =>
          import('./rfx-management/rfx-management.component').then((m) => m.RfxManagementComponent),
      },
      {
        path: 'rfx-management/create',
        title: 'Create RFx',
        canMatch: [procurementPermissionGuard],
        data: { permission: 'RFx Management', permissionAction: 'create' },
        loadComponent: () =>
          import('./rfx-management/create-rfx.component').then((m) => m.CreateRfxComponent),
      },
      {
        path: 'bid-evaluation',
        title: 'Bid Evaluation',
        canMatch: [procurementPermissionGuard],
        data: { permission: 'Bid Evaluation', permissionAction: 'read' },
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
        canMatch: [procurementPermissionGuard],
        data: { permission: 'Contract Management', permissionAction: 'read' },
        loadComponent: () =>
          import('./contract-management/contract-management.component').then((m) => m.ContractManagementComponent),
      },
      {
        path: 'supplier-performance',
        title: 'Supplier Performance',
        canMatch: [procurementPermissionGuard],
        data: { permission: 'Supplier Performance', permissionAction: 'read' },
        loadComponent: () =>
          import('./supplier-performance/supplier-performance.component').then((m) => m.SupplierPerformanceComponent),
      },
      {
        path: 'reports-analytics',
        title: 'Reports & Analytics',
        canMatch: [procurementPermissionGuard],
        data: { permission: 'Reports & Analytics', permissionAction: 'read' },
        loadComponent: () =>
          import('./reports-analytics/reports-analytics.component').then((m) => m.ReportsAnalyticsComponent),
      },
      {
        path: 'roles-permissions',
        title: 'Roles & Permissions',
        canMatch: [procurementPermissionGuard],
        data: { permission: 'Roles & Permissions', permissionAction: 'write' },
        loadComponent: () =>
          import('./roles-permissions/roles-permissions.component').then((m) => m.RolesPermissionsComponent),
      },
      {
        path: 'settings',
        title: 'Procurement Settings',
        loadComponent: () => import('./settings/procurement-settings.component').then((m) => m.ProcurementSettingsComponent),
      },
    ],
  },
];
