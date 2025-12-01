import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface MetricTile {
  label: string;
  value: string;
  trend: string;
  trendDirection: 'up' | 'down' | 'flat';
}

interface RecentActivity {
  title: string;
  description: string;
  timestamp: string;
}

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  metricTiles: MetricTile[] = [
    { label: 'Active Users', value: '2,410', trend: '+4.2% vs last week', trendDirection: 'up' },
    { label: 'Pending Requests', value: '58', trend: '-6.4% vs last week', trendDirection: 'down' },
    { label: 'Avg. Response', value: '1.9h', trend: '+0.3h vs target', trendDirection: 'flat' },
    { label: 'Open Tasks', value: '124', trend: '+2.1% vs last week', trendDirection: 'up' },
  ];

  recentActivities: RecentActivity[] = [
    { title: 'Workflow published', description: 'Procurement approval workflow v3 is now live.', timestamp: '5 minutes ago' },
    { title: 'New supplier onboarded', description: 'Ibn Sina Medical Supplies was added by Jane Cooper.', timestamp: '24 minutes ago' },
    { title: 'Policy updated', description: 'Document retention policy revision requires acknowledgement.', timestamp: '1 hour ago' },
    { title: 'Audit trail export', description: 'Weekly compliance log exported to S3.', timestamp: '3 hours ago' },
  ];

  trackByLabel(_: number, tile: MetricTile): string {
    return tile.label;
  }

  trackByActivity(_: number, activity: RecentActivity): string {
    return activity.title;
  }
}
