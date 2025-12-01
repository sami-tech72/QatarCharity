import { Routes } from '@angular/router';
import { adminRoutes } from './roles/admin/admin.routes';
import { procurementRoutes } from './roles/procurement/procurement.routes';
import { supplierRoutes } from './roles/supplier/supplier.routes';

export const routes: Routes = [
  ...adminRoutes,
  ...procurementRoutes,
  ...supplierRoutes,
  { path: '', pathMatch: 'full', redirectTo: 'admin/dashboard' },
  { path: 'dashboard', redirectTo: 'admin/dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'admin/dashboard' },
];
