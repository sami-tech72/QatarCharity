import { Component } from '@angular/core';

@Component({
  selector: 'app-system-settings-page',
  standalone: true,
  template: `
    <section class="card shadow-sm border-0">
      <div class="card-body py-10">
        <h2 class="fw-bold mb-4">System Settings</h2>
        <p class="text-muted mb-0">
          Adjust platform defaults, environment variables, and feature flags to
          align the experience with organizational standards.
        </p>
      </div>
    </section>
  `,
})
export class SystemSettingsComponent {}
