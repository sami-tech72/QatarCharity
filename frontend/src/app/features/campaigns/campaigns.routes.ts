import { Routes } from '@angular/router';

export const CAMPAIGN_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./campaigns.component').then((m) => m.CampaignsComponent)
  }
];
