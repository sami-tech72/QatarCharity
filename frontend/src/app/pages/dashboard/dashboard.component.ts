import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  template: `
    <section class="card shadow-sm border-0">
      <div class="card-body py-10">
        <h2 class="fw-bold mb-4">Dashboard</h2>
        <p class="text-muted mb-0">
          Review performance at a glance with streamlined panels that surface the
          most important activity for administrators.
        </p>
      </div>
    </section>
  `,
})
export class DashboardComponent {}
