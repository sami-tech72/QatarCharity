import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { LoginComponent } from './auth/login.component';
import { SidebarRole } from './components/sidebar/sidebar.component';
import { adminRoutes } from './roles/admin/admin.routes';
import { procurementRoutes } from './roles/procurement/procurement.routes';
import { supplierRoutes } from './roles/supplier/supplier.routes';

const withGuard = (role: SidebarRole, routes: Routes): Routes =>
  routes.map((route) => ({
    ...route,
    canMatch: [...(route.canMatch ?? []), authGuard],
    data: { ...(route.data ?? {}), role },
  }));

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  ...withGuard('Admin', adminRoutes),
  ...withGuard('Procurement', procurementRoutes),
  ...withGuard('Supplier', supplierRoutes),
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' },
];
