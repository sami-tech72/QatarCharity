import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

type WorkflowStatus = 'Active' | 'Draft' | 'Archived';

interface Workflow {
  name: string;
  stages: number;
  status: WorkflowStatus;
  lastModified: string;
}

@Component({
  selector: 'app-workflow-configuration',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './workflow-configuration.component.html',
  styleUrl: './workflow-configuration.component.scss',
})
export class WorkflowConfigurationComponent {
  readonly workflows: Workflow[] = [
    {
      name: 'Donation Approval',
      stages: 5,
      status: 'Active',
      lastModified: '2024-06-12T00:00:00Z',
    },
    {
      name: 'Volunteer Onboarding',
      stages: 3,
      status: 'Draft',
      lastModified: '2024-05-28T00:00:00Z',
    },
    {
      name: 'Expense Reimbursement',
      stages: 4,
      status: 'Active',
      lastModified: '2024-06-05T00:00:00Z',
    },
    {
      name: 'Grant Application Review',
      stages: 6,
      status: 'Archived',
      lastModified: '2024-04-18T00:00:00Z',
    },
  ];

  readonly statusBadgeClasses: Record<WorkflowStatus, string> = {
    Active: 'badge-light-success',
    Draft: 'badge-light-warning',
    Archived: 'badge-light-secondary',
  };

  trackByWorkflowName(_: number, workflow: Workflow): string {
    return workflow.name;
  }

  onEdit(workflow: Workflow): void {
    console.log('Edit workflow', workflow.name);
  }

  onDelete(workflow: Workflow): void {
    console.log('Delete workflow', workflow.name);
  }
}
