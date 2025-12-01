import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface Highlight {
  title: string;
  value: string;
  descriptor: string;
}

@Component({
  selector: 'app-procurement-dashboard-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './procurement-dashboard.component.html',
  styleUrl: './procurement-dashboard.component.scss',
})
export class ProcurementDashboardComponent {
  highlights: Highlight[] = [
    { title: 'Open RFxs', value: '12', descriptor: 'draft or in-market requests' },
    { title: 'Active Evaluations', value: '5', descriptor: 'tenders in scoring' },
    { title: 'Pending Approvals', value: '8', descriptor: 'awaiting committee review' },
  ];

  trackByTitle(_: number, highlight: Highlight): string {
    return highlight.title;
  }
}
