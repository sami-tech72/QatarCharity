import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { UserManagementService } from '../../../core/services/user-management.service';
import { CreateUserRequest, ManagedUser } from '../../../shared/models/user-management.model';
import { UserRole } from '../../../shared/models/user.model';

@Component({
  selector: 'app-user-management-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss',
})
export class UserManagementComponent implements OnInit {
  users: ManagedUser[] = [];
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

  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserManagementService);

  readonly userForm = this.fb.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: this.fb.nonNullable.control<UserRole>('Supplier'),
  });

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.userService.loadUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.isLoading = false;

        if (!users.length) {
          this.setAlert('No users found yet. Add your first collaborator to get started.', 'info');
        } else {
          this.alertMessage = '';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.setAlert(error.message || 'Failed to load users.', 'danger');
      },
    });
  }

  onSubmit(): void {
    if (this.userForm.invalid || this.isSubmitting) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const payload = this.userForm.getRawValue() as CreateUserRequest;

    this.userService.createUser(payload).subscribe({
      next: (user) => {
        this.users = [user, ...this.users.filter((existing) => existing.id !== user.id)];
        this.isSubmitting = false;
        this.resetForm();
        this.setAlert('User created successfully.', 'success');
      },
      error: (error) => {
        this.isSubmitting = false;
        this.setAlert(error.message || 'Unable to create user.', 'danger');
      },
    });
  }

  trackById(_: number, user: ManagedUser): string {
    return user.id;
  }

  private resetForm(): void {
    this.userForm.reset({
      displayName: '',
      email: '',
      password: '',
      role: 'Supplier',
    });
  }

  private setAlert(message: string, type: 'success' | 'danger' | 'info'): void {
    this.alertMessage = message;
    this.alertType = type;
  }
}
