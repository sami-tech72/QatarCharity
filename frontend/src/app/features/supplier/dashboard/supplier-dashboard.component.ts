import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface Snapshot {
  label: string;
  value: string;
}

@Component({
  selector: 'app-supplier-dashboard-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './supplier-dashboard.component.html',
  styleUrl: './supplier-dashboard.component.scss',
})
export class SupplierDashboardComponent {
  snapshots: Snapshot[] = [
    { label: 'Open Invitations', value: '4' },
    { label: 'Active Bids', value: '3' },
    { label: 'Contracts', value: '6' },
  ];

  trackByLabel(_: number, snapshot: Snapshot): string {
    return snapshot.label;
  }
}
