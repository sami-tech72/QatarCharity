import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface CampaignCard {
  name: string;
  summary: string;
  status: 'Active' | 'Planned' | 'Completed';
}

@Component({
  selector: 'app-campaigns',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './campaigns.component.html',
  styleUrl: './campaigns.component.scss'
})
export class CampaignsComponent {
  readonly campaigns: CampaignCard[] = [
    {
      name: 'Ramadan Food Support',
      summary: 'Coordinated distribution partners grouped in a feature folder with routing and UI.',
      status: 'Active'
    },
    {
      name: 'Education Access',
      summary: 'Standalone routes keep the code focused on the schooling domain and lazy-load on demand.',
      status: 'Planned'
    },
    {
      name: 'Water Security',
      summary: 'Shared helpers power UI reuse without exposing feature internals to other domains.',
      status: 'Completed'
    }
  ];
}
