import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface Integration {
  name: string;
  status: 'Connected' | 'Action required';
  detail: string;
}

@Component({
  selector: 'app-system-integrations-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './system-integrations.component.html',
  styleUrl: './system-integrations.component.scss',
})
export class SystemIntegrationsComponent {
  integrations: Integration[] = [
    { name: 'Microsoft 365', status: 'Connected', detail: 'SSO active — provisioning nightly at 1:00 AM' },
    { name: 'Slack', status: 'Connected', detail: 'Notifications enabled for approvals and alerts' },
    { name: 'Salesforce', status: 'Action required', detail: 'Refresh token expiring soon — reauthorize' },
    { name: 'DocuSign', status: 'Connected', detail: 'Contracts routed to legal group for signature' },
  ];

  trackByName(_: number, integration: Integration): string {
    return integration.name;
  }
}
