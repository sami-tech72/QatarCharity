import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Tender {
  title: string;
  reference: string;
  summary: string;
  submissionDeadline: string;
  budgetRange: string;
  publishedDate: string;
  remainingDays: number;
  type: string;
  category: string;
}

@Component({
  selector: 'app-available-tenders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './available-tenders.component.html',
  styleUrl: './available-tenders.component.scss',
})
export class AvailableTendersComponent {
  searchTerm = '';

  tenders: Tender[] = [
    {
      title: 'IT Infrastructure Upgrade',
      reference: 'RFX - 00012',
      summary: 'Request for proposals for upgrading our IT infrastructure.',
      submissionDeadline: '12/31/2024',
      budgetRange: '$100,000 - $500,000',
      publishedDate: '12/01/2024',
      remainingDays: 25,
      type: 'RFP',
      category: 'IT Infrastructure',
    },
    {
      title: 'Office Supplies Contract',
      reference: 'RFX - 00082',
      summary: 'Request for proposal for annual office supplies.',
      submissionDeadline: '12/16/2024',
      budgetRange: '$10,000 - $50,000',
      publishedDate: '12/01/2024',
      remainingDays: 10,
      type: 'RFO',
      category: 'Office Supplies',
    },
  ];

  get filteredTenders(): Tender[] {
    if (!this.searchTerm.trim()) {
      return this.tenders;
    }

    const term = this.searchTerm.trim().toLowerCase();

    return this.tenders.filter((tender) =>
      [
        tender.title,
        tender.reference,
        tender.category,
        tender.summary,
        tender.type,
      ]
        .join(' ')
        .toLowerCase()
        .includes(term)
    );
  }
}
