import { Component } from '@angular/core';

@Component({
  selector: 'app-document-templates-page',
  standalone: true,
  template: `
    <section class="card shadow-sm border-0">
      <div class="card-body py-10">
        <h2 class="fw-bold mb-4">Document Templates</h2>
        <p class="text-muted mb-0">
          Maintain reusable templates for proposals, agreements, and reviews so
          teams generate consistent documentation quickly.
        </p>
      </div>
    </section>
  `,
})
export class DocumentTemplatesComponent {}
