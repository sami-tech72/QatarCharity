import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-procurement-settings-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './procurement-settings.component.html',
  styleUrl: './procurement-settings.component.scss',
})
export class ProcurementSettingsComponent {
  settings = ['Approval workflows', 'Thresholds', 'Templates', 'Notifications'];
}
