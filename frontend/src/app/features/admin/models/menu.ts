import { SidebarMenuItem } from '../../../shared/components/sidebar/sidebar.component';

export const adminSidebarMenu: SidebarMenuItem[] = [
  { title: 'Dashboard', icon: 'ki-duotone ki-element-11 fs-2', path: '/admin/dashboard' },
  { title: 'User Management', icon: 'ki-duotone ki-people fs-2', path: '/admin/user-management' },
  { title: 'Supplier Management', icon: 'ki-duotone ki-briefcase fs-2', path: '/admin/supplier-management' },
  { title: 'Workflow Configuration', icon: 'ki-duotone ki-setting-3 fs-2', path: '/admin/workflow-configuration' },
  { title: 'Document Templates', icon: 'ki-duotone ki-folder fs-2', path: '/admin/document-templates' },
  { title: 'System Integrations', icon: 'ki-duotone ki-abstract-33 fs-2', path: '/admin/system-integrations' },
  { title: 'Audit Logs', icon: 'ki-duotone ki-shield-search fs-2', path: '/admin/audit-logs' },
  { title: 'System Settings', icon: 'ki-duotone ki-setting-2 fs-2', path: '/admin/system-settings' },
];
