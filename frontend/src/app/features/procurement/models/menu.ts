import { SidebarMenuItem } from '../../../core/layout/sidebar/sidebar.component';
import {
  PROCUREMENT_ALL_SUB_ROLES,
  PROCUREMENT_MANAGERS_AND_OFFICERS,
  PROCUREMENT_MANAGERS_ONLY,
} from './permissions';

export const procurementSidebarMenu: SidebarMenuItem[] = [
  { title: 'Dashboard', icon: 'ki-duotone ki-element-11 fs-2', path: '/procurement/dashboard' },
  {
    title: 'RFx Management',
    icon: 'ki-duotone ki-row-horizontal fs-2',
    path: '/procurement/rfx-management',
    allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS,
  },
  {
    title: 'Bid Evaluation',
    icon: 'ki-duotone ki-abstract-44 fs-2',
    path: '/procurement/bid-evaluation',
    allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS,
  },
  {
    title: 'Tender Committee',
    icon: 'ki-duotone ki-people fs-2',
    path: '/procurement/tender-committee',
    allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS,
  },
  {
    title: 'Contract Management',
    icon: 'ki-duotone ki-file-added fs-2',
    path: '/procurement/contract-management',
    allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS,
  },
  {
    title: 'Supplier Performance',
    icon: 'ki-duotone ki-activity fs-2',
    path: '/procurement/supplier-performance',
    allowedSubRoles: PROCUREMENT_MANAGERS_AND_OFFICERS,
  },
  {
    title: 'Reports & Analytics',
    icon: 'ki-duotone ki-chart-line-up fs-2',
    path: '/procurement/reports-analytics',
    allowedSubRoles: PROCUREMENT_ALL_SUB_ROLES,
  },
  {
    title: 'Settings',
    icon: 'ki-duotone ki-setting-2 fs-2',
    path: '/procurement/settings',
    allowedSubRoles: PROCUREMENT_MANAGERS_ONLY,
  },
];
