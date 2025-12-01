import { Component } from '@angular/core';

@Component({
  selector: 'app-audit-logs-page',
  standalone: true,
  template: `
    <section class="card shadow-sm border-0">
      <div class="card-body py-10">
        <h2 class="fw-bold mb-4">Audit Logs</h2>
        <p class="text-muted mb-0">
          Explore immutable activity logs to trace decisions, approvals, and
          changes for compliance reviews.
        </p>
      </div>
    </section>
  `,
})
export class AuditLogsComponent {}
