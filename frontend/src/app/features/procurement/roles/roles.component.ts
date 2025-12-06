import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

type PermissionKey = 'read' | 'write' | 'create';

type PermissionRow = {
  module: string;
  read: boolean;
  write: boolean;
  create: boolean;
};

type RoleCard = {
  name: string;
  users: number;
  avatars: string[];
  extraUsers?: number;
  badge?: string;
};

@Component({
  selector: 'app-procurement-roles',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './roles.component.html',
  styleUrls: ['./roles.component.scss'],
})
export class RolesComponent {
  protected roles: RoleCard[] = [
    {
      name: 'Administrator',
      users: 14,
      avatars: ['AN', 'MT', 'CR', 'HD'],
      extraUsers: 4,
      badge: 'Default',
    },
    {
      name: 'Manager',
      users: 9,
      avatars: ['LS', 'BK', 'AO', 'TT'],
      extraUsers: 2,
    },
    {
      name: 'Users',
      users: 7,
      avatars: ['GM', 'ID', 'RS', 'LP'],
      extraUsers: 3,
    },
    {
      name: 'Support',
      users: 5,
      avatars: ['CF', 'NI', 'JD', 'HB'],
    },
    {
      name: 'Restricted User',
      users: 4,
      avatars: ['TW', 'MG', 'CY', 'EL'],
    },
    {
      name: 'New Role',
      users: 2,
      avatars: ['AV', 'TR'],
      badge: 'Pending approval',
    },
  ];

  protected showAddRoleModal = false;
  protected newRoleName = '';
  protected adminAccess = false;
  protected selectAllPermissions = false;

  private readonly basePermissionRows: PermissionRow[] = [
    { module: 'User Management', read: true, write: true, create: true },
    { module: 'Content Management', read: true, write: true, create: false },
    { module: 'Disputes Management', read: true, write: false, create: false },
    { module: 'Database Management', read: true, write: true, create: true },
    { module: 'Email & Files', read: true, write: true, create: true },
    { module: 'Reporting', read: true, write: false, create: false },
    { module: 'API Control', read: true, write: true, create: true },
    { module: 'Repository Management', read: true, write: true, create: true },
    { module: 'Payroll', read: true, write: false, create: false },
  ];

  protected permissionRows: PermissionRow[] = this.basePermissionRows.map((row) => ({ ...row }));

  protected trackByRole(_: number, role: RoleCard): string {
    return role.name;
  }

  protected trackByPermissionRow(_: number, permission: PermissionRow): string {
    return permission.module;
  }

  protected openModal(): void {
    this.showAddRoleModal = true;
  }

  protected closeModal(): void {
    this.showAddRoleModal = false;
    this.resetForm();
  }

  protected toggleSelectAll(checked: boolean): void {
    this.permissionRows = this.permissionRows.map((row) => ({
      ...row,
      read: checked,
      write: checked,
      create: checked,
    }));
    this.selectAllPermissions = checked;
    this.adminAccess = checked;
  }

  protected togglePermission(row: PermissionRow, key: PermissionKey, checked: boolean): void {
    row[key] = checked;
    this.syncSelectAllState();
  }

  protected submitRole(): void {
    const trimmedName = this.newRoleName.trim();
    if (!trimmedName) {
      return;
    }

    this.roles.push({
      name: trimmedName,
      users: 0,
      avatars: [],
      badge: 'Draft',
    });

    this.closeModal();
  }

  protected onAdminAccessChange(checked: boolean): void {
    this.adminAccess = checked;
    this.toggleSelectAll(checked);
  }

  private resetForm(): void {
    this.newRoleName = '';
    this.adminAccess = false;
    this.selectAllPermissions = false;
    this.permissionRows = this.basePermissionRows.map((row) => ({ ...row }));
  }

  private syncSelectAllState(): void {
    const everyPermissionEnabled = this.permissionRows.every(
      (row) => row.read && row.write && row.create,
    );
    this.selectAllPermissions = everyPermissionEnabled;
    this.adminAccess = everyPermissionEnabled;
  }
}
