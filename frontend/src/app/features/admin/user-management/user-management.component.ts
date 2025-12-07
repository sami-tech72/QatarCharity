import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { Modal } from 'bootstrap';

import { UserManagementService } from '../../../core/services/user-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import {
  CreateUserRequest,
  ManagedUser,
  UserQueryRequest,
} from '../../../shared/models/user-management.model';
import { UserRole } from '../../../shared/models/user.model';
import { ProcurementRolesService } from '../../../core/services/procurement-roles.service';
import { ProcurementRolesResponse, ProcurementSubRole } from '../../../shared/models/procurement-roles.model';

@Component({
  selector: 'app-user-management-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss',
})
export class UserManagementComponent implements OnInit, OnDestroy {
  users: ManagedUser[] = [];
  usersPage: PagedResult<ManagedUser> | null = null;
  roleOptions: Array<{ value: UserRole; title: string; description: string }> = [
    {
      value: 'Admin',
      title: 'Administrator',
      description: 'Best for business owners and company administrators.',
    },
    {
      value: 'Procurement',
      title: 'Procurement',
      description: 'Manage procurement workflows and approvals.',
    },
    {
      value: 'Supplier',
      title: 'Supplier',
      description: 'Access supplier-specific tools and updates.',
    },
  ];
  isLoading = false;
  isSubmitting = false;
  deletingIds = new Set<string>();
  editingUser: ManagedUser | null = null;
  readonly pageSizes = [5, 10, 20, 50];
  readonly searchControl = new FormControl('', { nonNullable: true });
  procurementRoles: ProcurementSubRole[] = [];
  isLoadingProcurementRoles = false;
  private readonly destroy$ = new Subject<void>();

  paginationState: UserQueryRequest = {
    pageNumber: 1,
    pageSize: 10,
    search: '',
  };

  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserManagementService);
  private readonly notifier = inject(NotificationService);
  private readonly procurementRolesService = inject(ProcurementRolesService);

  readonly userForm = this.fb.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: this.fb.nonNullable.control<UserRole>('Supplier'),
    procurementRoleTemplateId: this.fb.control<number | null>(null),
  });

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((search) => {
        this.paginationState = { ...this.paginationState, pageNumber: 1, search: search.trim() };
        this.loadUsers();
      });

    this.userForm.controls.role.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((role) => this.updateProcurementRoleValidators(role));

    this.updateProcurementRoleValidators(this.userForm.controls.role.value);
    this.loadProcurementRoles();
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.userService.loadUsers(this.paginationState).subscribe({
      next: (page) => {
        this.usersPage = page;
        this.users = page.items;
        this.paginationState = {
          pageNumber: page.pageNumber,
          pageSize: page.pageSize,
          search: this.paginationState.search,
        };
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.notifier.error(this.getErrorMessage(error, 'Failed to load users.'));
      },
    });
  }

  onSubmit(): void {
    if (this.userForm.invalid || this.isSubmitting) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    if (this.editingUser) {
      const { displayName, email, role, procurementRoleTemplateId } = this.userForm.getRawValue();

      this.userService
        .updateUser(this.editingUser.id, { displayName, email, role, procurementRoleTemplateId })
        .subscribe({
        next: (user) => {
          this.users = this.users.map((existing) => (existing.id === user.id ? user : existing));
          this.refreshFromServer();
          this.finishSubmit('User updated successfully.', 'success');
        },
        error: (error) => {
          this.finishSubmit(this.getErrorMessage(error, 'Unable to update user.'), 'danger');
        },
      });
    } else {
      const payload = this.userForm.getRawValue() as CreateUserRequest;

      this.userService.createUser(payload).subscribe({
        next: (user) => {
          this.users = [user, ...this.users.filter((existing) => existing.id !== user.id)];
          this.refreshFromServer();
          this.finishSubmit('User created successfully.', 'success');
        },
        error: (error) => {
          this.finishSubmit(this.getErrorMessage(error, 'Unable to create user.'), 'danger');
        },
      });
    }
  }

  trackById(_: number, user: ManagedUser): string {
    return user.id;
  }

  deleteUser(user: ManagedUser): void {
    if (this.deletingIds.has(user.id)) {
      return;
    }

    const confirmed = window.confirm(`Are you sure you want to delete ${user.displayName}?`);

    if (!confirmed) {
      return;
    }

    this.deletingIds.add(user.id);

    this.userService.deleteUser(user.id).subscribe({
      next: () => {
        this.users = this.users.filter((existing) => existing.id !== user.id);
        this.deletingIds.delete(user.id);
        this.notifier.success('User deleted successfully.');
      },
      error: (error) => {
        this.deletingIds.delete(user.id);
        this.notifier.error(this.getErrorMessage(error, 'Unable to delete user.'));
      },
    });
  }

  startCreate(): void {
    this.editingUser = null;
    this.enablePasswordValidators();
    this.resetForm();
    this.updateProcurementRoleValidators(this.userForm.controls.role.value);
  }

  startEdit(user: ManagedUser): void {
    this.editingUser = user;
    this.disablePasswordValidators();
    this.userForm.patchValue({
      displayName: user.displayName,
      email: user.email,
      password: '',
      role: user.role,
      procurementRoleTemplateId: user.procurementRole?.id ?? null,
    });
    this.updateProcurementRoleValidators(user.role);
  }

  onCancel(): void {
    this.startCreate();
  }

  changePage(pageNumber: number): void {
    if (!this.usersPage) {
      return;
    }

    const safePage = Math.min(Math.max(pageNumber, 1), this.usersPage.totalPages || 1);

    if (safePage === this.paginationState.pageNumber) {
      return;
    }

    this.paginationState = { ...this.paginationState, pageNumber: safePage };
    this.loadUsers();
  }

  changePageSize(pageSize: string): void {
    const parsedSize = Number(pageSize) || this.paginationState.pageSize;
    this.paginationState = { ...this.paginationState, pageSize: parsedSize, pageNumber: 1 };
    this.loadUsers();
  }

  clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: true });
  }

  get totalPages(): number {
    return this.usersPage?.totalPages ?? 0;
  }

  get currentPage(): number {
    return this.usersPage?.pageNumber ?? this.paginationState.pageNumber;
  }

  get pageNumbers(): number[] {
    const total = this.totalPages;

    if (!total) {
      return [1];
    }

    const maxVisible = 5;
    const half = Math.floor(maxVisible / 2);
    let start = Math.max(1, this.currentPage - half);
    let end = start + maxVisible - 1;

    if (end > total) {
      end = total;
      start = Math.max(1, end - maxVisible + 1);
    }

    return Array.from({ length: end - start + 1 }, (_, idx) => start + idx);
  }

  private refreshFromServer(): void {
    this.loadUsers();
  }

  private resetForm(): void {
    this.userForm.reset({
      displayName: '',
      email: '',
      password: '',
      role: 'Supplier',
      procurementRoleTemplateId: null,
    });
  }

  private finishSubmit(message: string, type: 'success' | 'danger' | 'info'): void {
    this.isSubmitting = false;
    if (type === 'success') {
      this.hideModal('kt_modal_add_user');
      this.notifier.success(message);
    } else {
      this.notifier.error(message);
    }
    this.startCreate();
  }

  private hideModal(modalId: string): void {
    const element = document.getElementById(modalId);
    if (!element) {
      return;
    }

    const modal = Modal.getInstance(element) ?? new Modal(element);
    modal.hide();
  }

  private disablePasswordValidators(): void {
    const control = this.userForm.controls.password;
    control.clearValidators();
    control.updateValueAndValidity();
  }

  private enablePasswordValidators(): void {
    const control = this.userForm.controls.password;
    control.setValidators([Validators.required, Validators.minLength(6)]);
    control.updateValueAndValidity();
  }

  private updateProcurementRoleValidators(role: UserRole): void {
    const control = this.userForm.controls.procurementRoleTemplateId;

    if (role === 'Procurement') {
      control.setValidators([Validators.required]);
    } else {
      control.clearValidators();
      control.setValue(null, { emitEvent: false });
    }

    control.updateValueAndValidity({ emitEvent: false });
  }

  private loadProcurementRoles(): void {
    this.isLoadingProcurementRoles = true;

    this.procurementRolesService
      .loadRoles()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ProcurementRolesResponse) => {
          this.procurementRoles = response.subRoles;
          this.isLoadingProcurementRoles = false;
        },
        error: () => {
          this.procurementRoles = [];
          this.isLoadingProcurementRoles = false;
          this.notifier.error('Unable to load procurement sub-roles.');
        },
      });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    const apiError = (error as { error?: { message?: string }; message?: string }) || {};

    return apiError.error?.message || apiError.message || fallback;
  }
}
