import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ProcurementRolesService } from '../../../core/services/procurement-roles.service';
import {
  CreateProcurementRoleRequest,
  ProcurementPermission,
  ProcurementRolesResponse,
  ProcurementSubRole,
  UpdateProcurementRoleRequest,
} from '../../../shared/models/procurement-roles.model';
import { procurementSidebarMenu } from '../models/menu';

interface RoleCard {
  id: number;
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
  private subRoles: ProcurementSubRole[] = [];
  private defaultPermissions: Permission[] = [];
  isSubmitting = false;
  submissionError = '';
  editingRoleId: number | null = null;

  constructor(
    private readonly procurementRolesService: ProcurementRolesService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  openAddRoleModal(): void {
    this.startCreateRole();
  }

  closeAddRoleModal(): void {
    this.showAddRoleModal = false;
    this.resetRoleForm();
  }

  submitAddRole(): void {
    this.submissionError = '';

    if (!this.roleName.trim() || this.isSubmitting) {
      return;
    }

    const payload: CreateProcurementRoleRequest | UpdateProcurementRoleRequest = {
      name: this.roleName.trim(),
      description: 'Custom procurement role',
      permissions: this.permissions,
    };

    this.isSubmitting = true;

    const request$ = this.editingRoleId
      ? this.procurementRolesService.updateRole(this.editingRoleId, payload)
      : this.procurementRolesService.createRole(payload);

    request$.subscribe({
      next: (role) => {
        if (this.editingRoleId) {
          this.subRoles = this.subRoles.map((existing) => (existing.id === role.id ? role : existing));
        } else {
          this.subRoles = [...this.subRoles, role];
        }

        this.roles = this.subRoles.map((subRole) => this.mapToRoleCard(subRole));
        this.isSubmitting = false;
        this.closeAddRoleModal();
      },
      error: (error) => {
        this.isSubmitting = false;
        this.submissionError =
          error?.message || (this.editingRoleId ? 'Unable to update role at this time.' : 'Unable to create role at this time.');
      },
    });
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
    this.administratorAccess = allSelected;
  }

  private resetRoleForm(): void {
    this.editingRoleId = null;
    this.roleName = '';
    this.administratorAccess = false;
    this.submissionError = '';
    this.isSubmitting = false;
    this.permissions = this.createDefaultPermissions();
    this.updateSelectAllState();
  }

  private startCreateRole(): void {
    this.resetRoleForm();
    this.showAddRoleModal = true;
  }

  startEditRole(roleId: number): void {
    const role = this.subRoles.find((existing) => existing.id === roleId);

    if (!role) {
      return;
    }

    this.editingRoleId = role.id;
    this.roleName = role.name;
    this.permissions = this.mergeMenuPermissions(role.permissions);
    this.updateSelectAllState();
    this.showAddRoleModal = true;
  }

  addUserToRole(roleId: number): void {
    const role = this.subRoles.find((existing) => existing.id === roleId);

    if (!role) {
      return;
    }

    this.router.navigate(['/admin/user-management'], {
      queryParams: {
        role: 'Procurement',
        procurementRoleTemplateId: role.id,
      },
    });
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
        this.subRoles = response.subRoles;
        this.roles = this.subRoles.map((role) => this.mapToRoleCard(role));

        this.defaultPermissions = this.mergeMenuPermissions(response.defaultPermissions);
        this.permissions = this.createDefaultPermissions();
        this.updateSelectAllState();
      },
    });
  }

  private mergeMenuPermissions(sourcePermissions: Permission[]): Permission[] {
    const permissionsByName = new Map(sourcePermissions.map((permission) => [permission.name, permission.actions]));
    const defaultPermissionsByName = new Map(
      this.defaultPermissions.map((permission) => [permission.name, { ...permission.actions }]),
    );

    return procurementSidebarMenu.map((menuItem) => {
      const actions =
        permissionsByName.get(menuItem.title) ||
        defaultPermissionsByName.get(menuItem.title) ||
        ({ read: false, write: false, create: false } as ProcurementPermission['actions']);

      return {
        name: menuItem.title,
        actions: { ...actions },
      };
    });
  }

  private mapToRoleCard(role: ProcurementSubRole): RoleCard {
    return {
      id: role.id,
      name: role.name,
      description: role.description,
      newUsers: role.newUsers,
      totalUsers: role.totalUsers,
      avatars: role.avatars,
      extraCount: role.extraCount,
      actionLabel: 'Add user',
    };
  }
}

type Permission = ProcurementPermission;
