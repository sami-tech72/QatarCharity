import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';

import { UserManagementService } from '../../../core/services/user-management.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import {
  CreateUserRequest,
  ManagedUser,
  UserQueryRequest,
} from '../../../shared/models/user-management.model';
import { UserRole } from '../../../shared/models/user.model';

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
  alertMessage = '';
  alertType: 'success' | 'danger' | 'info' = 'info';
  deletingIds = new Set<string>();
  editingUser: ManagedUser | null = null;
  readonly pageSizes = [5, 10, 20, 50];
  readonly searchControl = new FormControl('', { nonNullable: true });
  private readonly destroy$ = new Subject<void>();

  paginationState: UserQueryRequest = {
    pageNumber: 1,
    pageSize: 10,
    search: '',
  };

  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserManagementService);

  readonly userForm = this.fb.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: this.fb.nonNullable.control<UserRole>('Supplier'),
  });

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((search) => {
        this.paginationState = { ...this.paginationState, pageNumber: 1, search: search.trim() };
        this.loadUsers();
      });

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

        if (!page.totalCount) {
          this.setAlert('No users found yet. Add your first collaborator to get started.', 'info');
        } else {
          this.alertMessage = '';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.setAlert(this.getErrorMessage(error, 'Failed to load users.'), 'danger');
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
      const { displayName, email, role } = this.userForm.getRawValue();

      this.userService.updateUser(this.editingUser.id, { displayName, email, role }).subscribe({
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
        this.setAlert('User deleted successfully.', 'success');
      },
      error: (error) => {
        this.deletingIds.delete(user.id);
        this.setAlert(this.getErrorMessage(error, 'Unable to delete user.'), 'danger');
      },
    });
  }

  startCreate(): void {
    this.editingUser = null;
    this.enablePasswordValidators();
    this.resetForm();
  }

  startEdit(user: ManagedUser): void {
    this.editingUser = user;
    this.disablePasswordValidators();
    this.userForm.patchValue({
      displayName: user.displayName,
      email: user.email,
      password: '',
      role: user.role,
    });
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
    });
  }

  private finishSubmit(message: string, type: 'success' | 'danger' | 'info'): void {
    this.isSubmitting = false;
    this.setAlert(message, type);
    this.startCreate();
  }

  private setAlert(message: string, type: 'success' | 'danger' | 'info'): void {
    this.alertMessage = message;
    this.alertType = type;
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

  private getErrorMessage(error: unknown, fallback: string): string {
    const apiError = (error as { error?: { message?: string }; message?: string }) || {};

    return apiError.error?.message || apiError.message || fallback;
  }
}
