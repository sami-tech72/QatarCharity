import { ChangeDetectionStrategy, Component } from '@angular/core';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [NgFor],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {
  readonly highlights = [
    { label: 'Active Campaigns', value: 12, detail: '+3 this week' },
    { label: 'Donors Engaged', value: 1840, detail: '78 new today' },
    { label: 'Regions Supported', value: 9, detail: 'Expanding coverage' },
  ];

  readonly quickActions = [
    'Create a campaign',
    'Review volunteer requests',
    'Publish a success story',
  ];
}
