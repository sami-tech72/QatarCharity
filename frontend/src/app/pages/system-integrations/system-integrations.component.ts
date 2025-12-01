import { Component } from '@angular/core';

@Component({
  selector: 'app-system-integrations-page',
  standalone: true,
  template: `
    <section class="card shadow-sm border-0">
      <div class="card-body py-10">
        <h2 class="fw-bold mb-4">System Integrations</h2>
        <p class="text-muted mb-0">
          Connect with external services, monitor sync health, and streamline
          data sharing between platforms.
        </p>
      </div>
    </section>
  `,
})
export class SystemIntegrationsComponent {}
