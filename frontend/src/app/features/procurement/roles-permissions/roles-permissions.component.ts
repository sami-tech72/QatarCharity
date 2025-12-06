import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProcurementRolesService } from '../../../core/services/procurement-roles.service';
import { ProcurementPermission, ProcurementRolesResponse } from '../../../shared/models/procurement-roles.model';

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
  imports: [CommonModule, FormsModule],
  templateUrl: './roles-permissions.component.html',
  styleUrl: './roles-permissions.component.scss',
})
export class RolesPermissionsComponent implements OnInit {
  showAddRoleModal = false;
  roleName = '';
  administratorAccess = false;
  selectAll = false;

  permissions: Permission[] = [];
  roles: RoleCard[] = [];
  mainRole = '';
  private defaultPermissions: Permission[] = [];

  constructor(private readonly procurementRolesService: ProcurementRolesService) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  openAddRoleModal(): void {
    this.showAddRoleModal = true;
  }

  closeAddRoleModal(): void {
    this.showAddRoleModal = false;
    this.resetRoleForm();
  }

  toggleAdministratorAccess(): void {
    this.administratorAccess = !this.administratorAccess;

    if (this.administratorAccess) {
      this.setAllPermissions(true);
    } else {
      this.updateSelectAllState();
    }
  }

  toggleSelectAll(): void {
    this.selectAll = !this.selectAll;
    this.setAllPermissions(this.selectAll);
  }

  onPermissionChange(): void {
    this.updateSelectAllState();
  }

  private setAllPermissions(value: boolean): void {
    this.permissions = this.permissions.map((permission) => ({
      ...permission,
      actions: {
        read: value,
        write: value,
        create: value,
      },
    }));

    this.updateSelectAllState();
  }

  private updateSelectAllState(): void {
    const allSelected = this.permissions.every(
      (permission) => permission.actions.read && permission.actions.write && permission.actions.create,
    );

    this.selectAll = allSelected;
  }

  private resetRoleForm(): void {
    this.roleName = '';
    this.administratorAccess = false;
    this.permissions = this.createDefaultPermissions();
    this.updateSelectAllState();
  }

  private createDefaultPermissions(): Permission[] {
    return this.defaultPermissions.map((permission) => ({
      name: permission.name,
      actions: { ...permission.actions },
    }));
  }

  private loadRoles(): void {
    this.procurementRolesService.loadRoles().subscribe({
      next: (response: ProcurementRolesResponse) => {
        this.mainRole = response.mainRole;
        this.roles = response.subRoles.map((role) => ({
          name: role.name,
          description: role.description,
          newUsers: role.newUsers,
          totalUsers: role.totalUsers,
          avatars: role.avatars,
          extraCount: role.extraCount,
          actionLabel: 'Add user',
        }));

        this.defaultPermissions = response.defaultPermissions;
        this.permissions = this.createDefaultPermissions();
        this.updateSelectAllState();
      },
    });
  }
}

type Permission = ProcurementPermission;
