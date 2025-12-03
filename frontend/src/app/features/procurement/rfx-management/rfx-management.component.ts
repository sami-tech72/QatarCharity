import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

interface RfxRecord {
  tenderId: string;
  title: string;
  category: string;
  status: 'Draft' | 'Published' | 'Closed';
  committeeStatus: 'Pending' | 'In Review' | 'Approved';
  submissionDeadline: string;
  estimatedValue: string;
}

@Component({
  selector: 'app-rfx-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './rfx-management.component.html',
  styleUrl: './rfx-management.component.scss',
})
export class RfxManagementComponent {
  readonly searchControl = new FormControl('', { nonNullable: true });

  readonly rfxRecords: RfxRecord[] = [
    {
      tenderId: 'RFQ-2024-001',
      title: 'Office Furniture Supply',
      category: 'Furniture',
      status: 'Published',
      committeeStatus: 'In Review',
      submissionDeadline: '2024-08-01',
      estimatedValue: '$120,000',
    },
    {
      tenderId: 'RFP-2024-014',
      title: 'IT Infrastructure Upgrade',
      category: 'Technology',
      status: 'Draft',
      committeeStatus: 'Pending',
      submissionDeadline: '2024-08-15',
      estimatedValue: '$450,000',
    },
    {
      tenderId: 'RFQ-2024-009',
      title: 'Vehicle Maintenance Services',
      category: 'Automotive',
      status: 'Published',
      committeeStatus: 'Approved',
      submissionDeadline: '2024-07-20',
      estimatedValue: '$95,000',
    },
    {
      tenderId: 'RFP-2024-020',
      title: 'Training & Development Program',
      category: 'Professional Services',
      status: 'Closed',
      committeeStatus: 'Approved',
      submissionDeadline: '2024-06-30',
      estimatedValue: '$60,000',
    },
  ];

  get filteredRecords(): RfxRecord[] {
    const term = this.searchControl.value.trim().toLowerCase();

    if (!term) {
      return this.rfxRecords;
    }

    return this.rfxRecords.filter((record) =>
      [
        record.tenderId,
        record.title,
        record.category,
        record.status,
        record.committeeStatus,
        record.submissionDeadline,
        record.estimatedValue,
      ]
        .filter(Boolean)
        .some((field) => field.toLowerCase().includes(term))
    );
  }

  get emptyStateMessage(): string {
    return this.searchControl.value ? 'No results match your search.' : 'No RFx records available yet.';
  }

  trackByTenderId(_: number, record: RfxRecord): string {
    return record.tenderId;
  }

  statusBadgeClass(status: RfxRecord['status']): string {
    switch (status) {
      case 'Published':
        return 'badge-light-success';
      case 'Closed':
        return 'badge-light-danger';
      default:
        return 'badge-light-warning';
    }
  }

  committeeBadgeClass(status: RfxRecord['committeeStatus']): string {
    switch (status) {
      case 'Approved':
        return 'badge-light-success';
      case 'In Review':
        return 'badge-light-info';
      default:
        return 'badge-light-warning';
    }
  }
}
