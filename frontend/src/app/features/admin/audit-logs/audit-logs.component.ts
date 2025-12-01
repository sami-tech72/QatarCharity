import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface AuditLog {
  actor: string;
  action: string;
  target: string;
  timestamp: string;
}

@Component({
  selector: 'app-audit-logs-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './audit-logs.component.html',
  styleUrl: './audit-logs.component.scss',
})
export class AuditLogsComponent {
  logs: AuditLog[] = [
    { actor: 'Jane Cooper', action: 'Updated role', target: 'Robert Fox → Reviewer', timestamp: '5 minutes ago' },
    { actor: 'Devon Lane', action: 'Exported audit trail', target: 'Last 30 days', timestamp: '22 minutes ago' },
    { actor: 'Courtney Henry', action: 'Acknowledged policy', target: 'Document retention', timestamp: '1 hour ago' },
    { actor: 'Robert Fox', action: 'Signed document', target: 'Supplier contract renewal', timestamp: '3 hours ago' },
  ];

  trackByTimestamp(_: number, log: AuditLog): string {
    return log.timestamp + log.actor;
  }
}
