import { SidebarMenuItem } from '../../../core/layout/sidebar/sidebar.component';

export const supplierSidebarMenu: SidebarMenuItem[] = [
  { title: 'Dashboard', icon: 'ki-duotone ki-element-11 fs-2', path: '/supplier/dashboard' },
  { title: 'Available Tenders', icon: 'ki-duotone ki-search-list fs-2', path: '/supplier/available-tenders' },
  { title: 'My Bids', icon: 'ki-duotone ki-send fs-2', path: '/supplier/my-bids' },
  { title: 'My Contracts', icon: 'ki-duotone ki-folder-up fs-2', path: '/supplier/my-contracts' },
  { title: 'Performance', icon: 'ki-duotone ki-chart-line-up fs-2', path: '/supplier/performance' },
  { title: 'Company Profile', icon: 'ki-duotone ki-profile-user fs-2', path: '/supplier/company-profile' },
  { title: 'Documents', icon: 'ki-duotone ki-file-up fs-2', path: '/supplier/documents' },
  { title: 'Settings', icon: 'ki-duotone ki-setting-2 fs-2', path: '/supplier/settings' },
];
