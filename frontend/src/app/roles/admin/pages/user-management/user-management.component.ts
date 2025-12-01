import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface UserRow {
  name: string;
  role: string;
  status: 'Active' | 'Invited' | 'Suspended';
  lastActive: string;
}

@Component({
  selector: 'app-user-management-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss',
})
export class UserManagementComponent {
  users: UserRow[] = [
    { name: 'Jane Cooper', role: 'Administrator', status: 'Active', lastActive: '1 minute ago' },
    { name: 'Devon Lane', role: 'Approver', status: 'Active', lastActive: '12 minutes ago' },
    { name: 'Darlene Robertson', role: 'Finance', status: 'Invited', lastActive: 'Pending' },
    { name: 'Robert Fox', role: 'Reviewer', status: 'Suspended', lastActive: '3 weeks ago' },
    { name: 'Courtney Henry', role: 'Requester', status: 'Active', lastActive: '2 hours ago' },
  ];

  trackByName(_: number, row: UserRow): string {
    return row.name;
  }
}
