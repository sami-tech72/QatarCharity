import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-rfx-management-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rfx-management.component.html',
  styleUrl: './rfx-management.component.scss',
})
export class RfxManagementComponent {
  phases = ['Drafting', 'Approvals', 'In Market', 'Clarifications', 'Closed'];
}
