import { Component } from '@angular/core';

@Component({
  selector: 'app-user-management-page',
  standalone: true,
  template: `
    <section class="card shadow-sm border-0">
      <div class="card-body py-10">
        <h2 class="fw-bold mb-4">User Management</h2>
        <p class="text-muted mb-0">
          Manage roles, permissions, and access policies with clear visibility
          into who can do what across the platform.
        </p>
      </div>
    </section>
  `,
})
export class UserManagementComponent {}
