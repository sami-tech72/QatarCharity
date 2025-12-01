import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface TemplateCard {
  title: string;
  category: string;
  updated: string;
}

@Component({
  selector: 'app-document-templates-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './document-templates.component.html',
  styleUrl: './document-templates.component.scss',
})
export class DocumentTemplatesComponent {
  templates: TemplateCard[] = [
    { title: 'Procurement request', category: 'Operations', updated: 'Updated 1 day ago' },
    { title: 'Vendor onboarding', category: 'Compliance', updated: 'Updated 6 days ago' },
    { title: 'Travel advance', category: 'Finance', updated: 'Updated 2 weeks ago' },
    { title: 'Project charter', category: 'Projects', updated: 'Updated 1 month ago' },
  ];

  trackByTitle(_: number, template: TemplateCard): string {
    return template.title;
  }
}
