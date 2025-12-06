import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Modal } from 'bootstrap';

type PermissionAction = 'read' | 'write' | 'create';

type PermissionMatrix = Record<string, Record<PermissionAction, boolean>>;

interface PermissionCategory {
  key: string;
  title: string;
}

interface ProcurementRole {
  name: string;
  description: string;
  usersCount: number;
  requests: number;
  userAvatars: string[];
  permissions: PermissionMatrix;
}

@Component({
  selector: 'app-procurement-roles-permissions',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './roles-permissions.component.html',
  styleUrl: './roles-permissions.component.scss',
})
export class ProcurementRolesPermissionsComponent implements OnInit {
  @ViewChild('roleModalRef') roleModalRef?: ElementRef<HTMLDivElement>;

  readonly permissionCategories: PermissionCategory[] = [
    { key: 'userManagement', title: 'User Management' },
    { key: 'contentManagement', title: 'Content Management' },
    { key: 'disputesManagement', title: 'Disputes Management' },
    { key: 'databaseManagement', title: 'Database Management' },
    { key: 'financialControl', title: 'Financial Control' },
    { key: 'reporting', title: 'Reporting' },
    { key: 'apiControl', title: 'API Control' },
    { key: 'repositoryManagement', title: 'Repository Management' },
    { key: 'payroll', title: 'Payroll' },
  ];

  roles: ProcurementRole[] = [
    {
      name: 'Administrator',
      description: 'Best for business owners and company administrators.',
      usersCount: 5,
      requests: 3,
      userAvatars: [
        'assets/media/avatars/300-6.jpg',
        'assets/media/avatars/300-5.jpg',
        'assets/media/avatars/300-11.jpg',
        'assets/media/avatars/300-9.jpg',
      ],
      permissions: this.createFullAccess(),
    },
    {
      name: 'Manager',
      description: 'Best for small business owners and managers.',
      usersCount: 5,
      requests: 2,
      userAvatars: [
        'assets/media/avatars/300-23.jpg',
        'assets/media/avatars/300-12.jpg',
        'assets/media/avatars/300-9.jpg',
      ],
      permissions: this.createDefaultPermissions(['read', 'write']),
    },
    {
      name: 'Users',
      description: 'Best for content creators needing simple access.',
      usersCount: 5,
      requests: 0,
      userAvatars: [
        'assets/media/avatars/300-3.jpg',
        'assets/media/avatars/300-20.jpg',
        'assets/media/avatars/300-23.jpg',
        'assets/media/avatars/300-10.jpg',
      ],
      permissions: this.createDefaultPermissions(['read']),
    },
    {
      name: 'Support',
      description: 'Best for support staff needing access to reports.',
      usersCount: 5,
      requests: 0,
      userAvatars: ['assets/media/avatars/300-1.jpg', 'assets/media/avatars/300-9.jpg', 'assets/media/avatars/300-21.jpg'],
      permissions: this.createDefaultPermissions(['read', 'write']),
    },
    {
      name: 'Restricted User',
      description: 'Best for teams in a contributing role.',
      usersCount: 5,
      requests: 0,
      userAvatars: ['assets/media/avatars/300-14.jpg', 'assets/media/avatars/300-2.jpg'],
      permissions: this.createDefaultPermissions(['read']),
    },
  ];

  editingRole: ProcurementRole | null = null;

  readonly roleForm: FormGroup;

  private readonly fb = inject(FormBuilder);

  constructor() {
    this.roleForm = this.fb.group({
      roleName: ['', Validators.required],
      administratorAccess: [false],
      permissions: this.buildPermissionsGroup(),
    });
  }

  ngOnInit(): void {
    this.roleForm
      .get('administratorAccess')
      ?.valueChanges.subscribe((isAdmin: boolean) => this.handleAdministratorToggle(isAdmin));
  }

  startCreate(): void {
    this.editingRole = null;
    this.roleForm.reset({
      roleName: '',
      administratorAccess: false,
    });
    this.resetPermissions(false);
  }

  startEdit(role: ProcurementRole): void {
    this.editingRole = role;
    this.roleForm.patchValue({
      roleName: role.name,
      administratorAccess: this.isFullAccess(role.permissions),
    });

    this.permissionCategories.forEach((category) => {
      const categoryGroup = this.getPermissionGroup(category.key);
      const currentPermissions = role.permissions[category.key];
      categoryGroup.patchValue({
        read: currentPermissions.read,
        write: currentPermissions.write,
        create: currentPermissions.create,
      });
    });
  }

  getPermissionGroup(key: string): FormGroup {
    return this.roleForm.get(['permissions', key]) as FormGroup;
  }

  toggleSelectAll(selectAll: boolean): void {
    this.resetPermissions(selectAll);
    this.roleForm.patchValue({ administratorAccess: selectAll });
  }

  isSelectAllChecked(): boolean {
    return this.permissionCategories.every((category) => {
      const group = this.getPermissionGroup(category.key);
      return group.value.read && group.value.write && group.value.create;
    });
  }

  saveRole(): void {
    if (this.roleForm.invalid) {
      this.roleForm.markAllAsTouched();
      return;
    }

    const formValue = this.roleForm.getRawValue();
    const permissions = formValue.permissions as PermissionMatrix;

    if (this.editingRole) {
      this.editingRole.name = formValue.roleName;
      this.editingRole.permissions = permissions;
    } else {
      const newRole: ProcurementRole = {
        name: formValue.roleName,
        description: 'Custom procurement role with tailored permissions.',
        usersCount: 0,
        requests: 0,
        userAvatars: ['assets/media/avatars/300-1.jpg'],
        permissions,
      };
      this.roles = [newRole, ...this.roles];
    }

    this.startCreate();
    this.closeModal();
  }

  trackByRole(_: number, role: ProcurementRole): string {
    return role.name;
  }

  private buildPermissionsGroup(): FormGroup {
    const groupConfig: Record<string, FormGroup> = {} as Record<string, FormGroup>;

    this.permissionCategories.forEach((category) => {
      groupConfig[category.key] = this.fb.group({
        read: [false],
        write: [false],
        create: [false],
      });
    });

    return this.fb.group(groupConfig);
  }

  private createDefaultPermissions(actions: PermissionAction[]): PermissionMatrix {
    const matrix: PermissionMatrix = {};

    this.permissionCategories.forEach((category) => {
      matrix[category.key] = {
        read: actions.includes('read'),
        write: actions.includes('write'),
        create: actions.includes('create'),
      };
    });

    return matrix;
  }

  private createFullAccess(): PermissionMatrix {
    return this.createDefaultPermissions(['read', 'write', 'create']);
  }

  private resetPermissions(value: boolean): void {
    this.permissionCategories.forEach((category) => {
      this.getPermissionGroup(category.key).setValue({
        read: value,
        write: value,
        create: value,
      });
    });
  }

  private handleAdministratorToggle(isAdmin: boolean): void {
    if (isAdmin) {
      this.resetPermissions(true);
    }
  }

  private isFullAccess(matrix: PermissionMatrix): boolean {
    return this.permissionCategories.every((category) => {
      const permission = matrix[category.key];
      return permission.read && permission.write && permission.create;
    });
  }

  private closeModal(): void {
    if (!this.roleModalRef?.nativeElement) {
      return;
    }

    const modal = Modal.getInstance(this.roleModalRef.nativeElement) ??
      new Modal(this.roleModalRef.nativeElement);

    modal.hide();
  }
}
