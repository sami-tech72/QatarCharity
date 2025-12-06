import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

type PermissionType = 'View' | 'Edit' | 'Create' | 'Delete';

type MenuPermission = {
  menu: string;
  view: boolean;
  edit: boolean;
  create: boolean;
  delete: boolean;
};

type SubRole = {
  name: string;
  description: string;
  members: number;
  permissions: PermissionType[];
};

@Component({
  selector: 'app-procurement-roles',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './roles.component.html',
  styleUrls: ['./roles.component.scss'],
})
export class RolesComponent {
  protected readonly permissionTypes: PermissionType[] = ['View', 'Edit', 'Create', 'Delete'];

  protected readonly mainRole = {
    name: 'Procurement',
    members: 18,
    description:
      'Primary role with visibility across procurement sourcing, evaluation, contracting, and performance oversight.',
    permissions: ['View', 'Edit', 'Create'] as PermissionType[],
  };

  protected readonly subRoles: SubRole[] = [
    {
      name: 'Category Manager',
      members: 6,
      description: 'Owns sourcing cycles, category strategies, and approvals for assigned spend areas.',
      permissions: ['View', 'Edit', 'Create'],
    },
    {
      name: 'Sourcing Analyst',
      members: 4,
      description: 'Prepares RFx events, runs evaluations, and manages supplier responses.',
      permissions: ['View', 'Edit', 'Create'],
    },
    {
      name: 'Committee Reviewer',
      members: 5,
      description: 'Reviews submissions and records committee decisions for each tender.',
      permissions: ['View', 'Edit'],
    },
    {
      name: 'Auditor',
      members: 3,
      description: 'Observes procurement activities and reporting with read-only access.',
      permissions: ['View'],
    },
  ];

  protected readonly menuPermissions: MenuPermission[] = [
    { menu: 'Dashboard', view: true, edit: false, create: false, delete: false },
    { menu: 'RFx Management', view: true, edit: true, create: true, delete: true },
    { menu: 'Bid Evaluation', view: true, edit: true, create: true, delete: false },
    { menu: 'Tender Committee', view: true, edit: true, create: false, delete: false },
    { menu: 'Contract Management', view: true, edit: true, create: true, delete: true },
    { menu: 'Supplier Performance', view: true, edit: true, create: false, delete: false },
    { menu: 'Reports & Analytics', view: true, edit: false, create: false, delete: false },
    { menu: 'Members', view: true, edit: true, create: true, delete: true },
    { menu: 'Roles & Permissions', view: true, edit: true, create: true, delete: true },
    { menu: 'Settings', view: true, edit: true, create: false, delete: false },
  ];
}
