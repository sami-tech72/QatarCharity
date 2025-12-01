import { Routes } from '@angular/router';
import { adminRoutes } from './roles/admin/admin.routes';
import { procurementRoutes } from './roles/procurement/procurement.routes';
import { supplierRoutes } from './roles/supplier/supplier.routes';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Login',
    loadComponent: () => import('./components/login/login.component').then((m) => m.LoginComponent),
  },
  ...adminRoutes,
  ...procurementRoutes,
  ...supplierRoutes,
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'dashboard', redirectTo: 'admin/dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' },
];
