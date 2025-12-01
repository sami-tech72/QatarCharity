import { Routes } from '@angular/router';

export const adminRoutes: Routes = [
  {
    path: 'admin',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Admin Dashboard',
        loadComponent: () => import('./dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'user-management',
        title: 'User Management',
        loadComponent: () => import('./user-management/user-management.component').then((m) => m.UserManagementComponent),
      },
      {
        path: 'supplier-management',
        title: 'Supplier Management',
        loadComponent: () =>
          import('./supplier-management/supplier-management.component').then((m) => m.SupplierManagementComponent),
      },
      {
        path: 'workflow-configuration',
        title: 'Workflow Configuration',
        loadComponent: () =>
          import('./workflow-configuration/workflow-configuration.component').then((m) => m.WorkflowConfigurationComponent),
      },
      {
        path: 'document-templates',
        title: 'Document Templates',
        loadComponent: () =>
          import('./document-templates/document-templates.component').then((m) => m.DocumentTemplatesComponent),
      },
      {
        path: 'system-integrations',
        title: 'System Integrations',
        loadComponent: () =>
          import('./system-integrations/system-integrations.component').then((m) => m.SystemIntegrationsComponent),
      },
      {
        path: 'audit-logs',
        title: 'Audit Logs',
        loadComponent: () => import('./audit-logs/audit-logs.component').then((m) => m.AuditLogsComponent),
      },
      {
        path: 'system-settings',
        title: 'System Settings',
        loadComponent: () =>
          import('./system-settings/system-settings.component').then((m) => m.SystemSettingsComponent),
      },
    ],
  },
];
