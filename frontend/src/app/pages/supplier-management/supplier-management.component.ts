import { Component } from '@angular/core';

@Component({
  selector: 'app-supplier-management-page',
  standalone: true,
  template: `
    <section class="card shadow-sm border-0">
      <div class="card-body py-10">
        <h2 class="fw-bold mb-4">Supplier Management</h2>
        <p class="text-muted mb-0">
          Track vendor performance, onboarding, and compliance to keep
          procurement workflows aligned and auditable.
        </p>
      </div>
    </section>
  `,
})
export class SupplierManagementComponent {}
