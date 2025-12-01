import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { SidebarRole } from './shared/components/sidebar/sidebar.component';
import { authRoutes } from './features/auth/auth.routes';
import { adminRoutes } from './features/admin/admin.routes';
import { procurementRoutes } from './features/procurement/procurement.routes';
import { supplierRoutes } from './features/supplier/supplier.routes';

const withGuard = (role: SidebarRole, routes: Routes): Routes =>
  routes.map((route) => ({
    ...route,
    canMatch: [...(route.canMatch ?? []), authGuard],
    data: { ...(route.data ?? {}), role },
  }));

export const routes: Routes = [
  ...authRoutes,
  ...withGuard('Admin', adminRoutes),
  ...withGuard('Procurement', procurementRoutes),
  ...withGuard('Supplier', supplierRoutes),
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' },
];
