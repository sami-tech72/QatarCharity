import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-supplier-settings-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './supplier-settings.component.html',
  styleUrl: './supplier-settings.component.scss',
})
export class SupplierSettingsComponent {
  preferences = ['Notifications', 'Team members', 'Profiles', 'Security'];
}
