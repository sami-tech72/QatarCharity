import { Component } from '@angular/core';

@Component({
  selector: 'app-workflow-configuration-page',
  standalone: true,
  template: `
    <section class="card shadow-sm border-0">
      <div class="card-body py-10">
        <h2 class="fw-bold mb-4">Workflow Configuration</h2>
        <p class="text-muted mb-0">
          Configure approval chains, automation rules, and escalation paths to
          keep organizational processes consistent.
        </p>
      </div>
    </section>
  `,
})
export class WorkflowConfigurationComponent {}
