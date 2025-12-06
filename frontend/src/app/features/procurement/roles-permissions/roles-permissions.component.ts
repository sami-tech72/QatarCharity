import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface RoleCard {
  name: string;
  totalUsers: number;
  newUsers: number;
  description: string;
  avatars: string[];
  extraCount?: number;
  actionLabel: string;
}

@Component({
  selector: 'app-procurement-roles-permissions-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './roles-permissions.component.html',
  styleUrl: './roles-permissions.component.scss',
})
export class RolesPermissionsComponent {
  readonly roles: RoleCard[] = [
    {
      name: 'Administrator',
      totalUsers: 4,
      newUsers: 2,
      description: 'Best for business owners and company administrators',
      avatars: ['300-6.jpg', '300-5.jpg', '300-11.jpg', '300-3.jpg'],
      actionLabel: 'Add user',
    },
    {
      name: 'Manager',
      totalUsers: 5,
      newUsers: 2,
      description: 'Best for team leads to manage permissions',
      avatars: ['300-14.jpg', '300-2.jpg', '300-7.jpg', '300-8.jpg'],
      extraCount: 1,
      actionLabel: 'Add user',
    },
    {
      name: 'Users',
      totalUsers: 8,
      newUsers: 4,
      description: 'Best for standard users who need access to all standard features.',
      avatars: ['300-9.jpg', '300-10.jpg', '300-12.jpg', '300-13.jpg'],
      extraCount: 2,
      actionLabel: 'Add user',
    },
    {
      name: 'Support',
      totalUsers: 3,
      newUsers: 2,
      description: 'Best for employees who regularly refund payments',
      avatars: ['300-4.jpg', '300-1.jpg', '300-19.jpg'],
      actionLabel: 'Add user',
    },
    {
      name: 'Restricted User',
      totalUsers: 4,
      newUsers: 1,
      description: 'Best for people who need restricted access to sensitive data',
      avatars: ['300-21.jpg', '300-23.jpg', '300-24.jpg', '300-25.jpg'],
      actionLabel: 'Add user',
    },
  ];
}
