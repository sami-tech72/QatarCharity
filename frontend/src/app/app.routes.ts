import { Routes } from '@angular/router';
import { AppShellComponent } from './core/layout/app-shell.component';

export const routes: Routes = [
  {
    path: '',
    component: AppShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      {
        path: 'home',
        loadChildren: () => import('./features/home/home.routes').then((m) => m.HOME_ROUTES)
      },
      {
        path: 'campaigns',
        loadChildren: () => import('./features/campaigns/campaigns.routes').then((m) => m.CAMPAIGN_ROUTES)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
