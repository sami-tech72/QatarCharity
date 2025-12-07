import { SidebarMenuItem } from '../../../core/layout/sidebar/sidebar.component';

export const procurementSidebarMenu: SidebarMenuItem[] = [
  { title: 'Dashboard', icon: 'ki-duotone ki-element-11 fs-2', path: '/procurement/dashboard' },
  {
    title: 'RFx Management',
    icon: 'ki-duotone ki-row-horizontal fs-2',
    path: '/procurement/rfx-management',
    permission: { name: 'RFx Management', action: 'read' },
  },
  {
    title: 'Bid Evaluation',
    icon: 'ki-duotone ki-abstract-44 fs-2',
    path: '/procurement/bid-evaluation',
    permission: { name: 'Bid Evaluation', action: 'read' },
  },
  { title: 'Tender Committee', icon: 'ki-duotone ki-people fs-2', path: '/procurement/tender-committee' },
  {
    title: 'Contract Management',
    icon: 'ki-duotone ki-file-added fs-2',
    path: '/procurement/contract-management',
    permission: { name: 'Contract Management', action: 'read' },
  },
  {
    title: 'Supplier Performance',
    icon: 'ki-duotone ki-activity fs-2',
    path: '/procurement/supplier-performance',
    permission: { name: 'Supplier Performance', action: 'read' },
  },
  {
    title: 'Reports & Analytics',
    icon: 'ki-duotone ki-chart-line-up fs-2',
    path: '/procurement/reports-analytics',
    permission: { name: 'Reports & Analytics', action: 'read' },
  },
  {
    title: 'Roles & Permissions',
    icon: 'ki-duotone ki-profile-user fs-2',
    path: '/procurement/roles-permissions',
    permission: { name: 'Roles & Permissions', action: 'write' },
  },
  { title: 'Settings', icon: 'ki-duotone ki-setting-2 fs-2', path: '/procurement/settings' },
];
