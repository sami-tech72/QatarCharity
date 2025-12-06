import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RolesService } from './roles.service';
import { ProcurementRolePayload } from './models/procurement-role.model';

type PermissionKey = 'view' | 'edit' | 'create' | 'delete';

type PermissionRow = {
  menu: string;
  view: boolean;
  edit: boolean;
  create: boolean;
  delete: boolean;
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
export class RolesComponent implements OnInit {
  @ViewChild('roleNameInput')
  public roleNameInput?: ElementRef<HTMLInputElement>;

  public mainRole = 'Procurement';
  public roles: RoleCard[] = [];

  public showAddRoleModal = false;
  public newRoleName = '';
  public adminAccess = false;
  public selectAllPermissions = false;
  public loading = false;

  private basePermissionRows: PermissionRow[] = [];

  public permissionRows: PermissionRow[] = [];

  constructor(private readonly rolesService: RolesService) {}

  public ngOnInit(): void {
    this.loadRoles();
  }

  public trackByRole(_: number, role: RoleCard): string {
    return role.name;
  }

  public trackByPermissionRow(_: number, permission: PermissionRow): string {
    return permission.menu;
  }

  public openModal(): void {
    this.showAddRoleModal = true;
    // Reset the form so repeated opens always start clean and ensure focus once the view renders.
    this.resetForm();
    setTimeout(() => this.roleNameInput?.nativeElement.focus());
  }

  public closeModal(): void {
    this.showAddRoleModal = false;
    this.resetForm();
  }

  public toggleSelectAll(checked: boolean): void {
    this.permissionRows = this.permissionRows.map((row) => ({
      ...row,
      view: checked,
      edit: checked,
      create: checked,
      delete: checked,
    }));
    this.selectAllPermissions = checked;
    this.adminAccess = checked;
  }

  public togglePermission(row: PermissionRow, key: PermissionKey, checked: boolean): void {
    row[key] = checked;
    this.syncSelectAllState();
  }

  public submitRole(): void {
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

  public onAdminAccessChange(checked: boolean): void {
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
      (row) => row.view && row.edit && row.create && row.delete,
    );
    this.selectAllPermissions = everyPermissionEnabled;
    this.adminAccess = everyPermissionEnabled;
  }

  private loadRoles(): void {
    this.loading = true;
    this.rolesService.getProcurementRoles().subscribe({
      next: (payload: ProcurementRolePayload) => {
        this.applyPayload(payload);
        this.loading = false;
      },
      error: () => {
        this.applyPayload({ mainRole: 'Procurement', subRoles: [], menuPermissions: [] });
        this.loading = false;
      },
    });
  }

  private applyPayload(payload: ProcurementRolePayload): void {
    this.mainRole = payload.mainRole;
    this.roles = payload.subRoles.map((role) => ({
      name: role.name,
      users: role.users,
      avatars: role.avatars,
      extraUsers: role.extraUsers,
      badge: role.badge,
    }));

    this.basePermissionRows = payload.menuPermissions.map((permission) => ({ ...permission }));
    this.permissionRows = this.basePermissionRows.map((row) => ({ ...row }));
  }
}
