import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface SettingToggle {
  label: string;
  description: string;
  enabled: boolean;
}

@Component({
  selector: 'app-system-settings-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './system-settings.component.html',
  styleUrl: './system-settings.component.scss',
})
export class SystemSettingsComponent {
  toggles: SettingToggle[] = [
    {
      label: 'Require MFA for all administrators',
      description: 'Enforce time-based one-time passwords for console access.',
      enabled: true,
    },
    {
      label: 'Send weekly operational digest',
      description: 'Email key metrics and SLA summaries to platform owners.',
      enabled: true,
    },
    {
      label: 'Auto-archive inactive requests',
      description: 'Move inactive items to cold storage after 180 days.',
      enabled: false,
    },
  ];

  trackByLabel(_: number, toggle: SettingToggle): string {
    return toggle.label;
  }
}
